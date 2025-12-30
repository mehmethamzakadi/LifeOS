using LifeOS.Application.Abstractions;
using LifeOS.Application.Abstractions.Identity;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Exceptions;
using LifeOS.Domain.Services;
using LifeOS.Infrastructure.Extensions;
using LifeOS.Persistence.Contexts;
using AppPasswordHasher = LifeOS.Application.Abstractions.Identity.IPasswordHasher;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly LifeOSDbContext _context;
    private readonly IUserDomainService _userDomainService;
    private readonly ITokenService _tokenService;
    private readonly IMailService _mailService;
    private readonly AppPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        LifeOSDbContext context,
        IUserDomainService userDomainService,
        ITokenService tokenService,
        IMailService mailService,
        AppPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _context = context;
        _userDomainService = userDomainService;
        _tokenService = tokenService;
        _mailService = mailService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<IDataResult<AuthResult>> LoginAsync(string email, string password, string? deviceId = null)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted)
            ?? throw new AuthenticationErrorException();

        // Check if account is locked
        if (user.IsLockedOut())
        {
            throw new AuthenticationErrorException("Hesabınız çok sayıda hatalı giriş nedeniyle kilitlendi.");
        }

        // Check if two factor is enabled
        if (user.TwoFactorEnabled)
        {
            throw new AuthenticationErrorException("İki faktörlü doğrulama gereklidir.");
        }

        // Verify password
        if (!_userDomainService.VerifyPassword(user, password))
        {
            // ✅ SECURITY: Using User entity behavior method for failed access tracking
            user.IncrementAccessFailedCount();

            // Lock account after 5 failed attempts
            if (user.AccessFailedCount >= 5)
            {
                user.LockAccount(DateTimeOffset.UtcNow.AddMinutes(15));
            }

            _context.Users.Update(user);
            await SaveChangesWithConcurrencyHandlingAsync();

            throw new AuthenticationErrorException();
        }

        // ✅ SECURITY: Reset failed access count on successful login
        user.ResetAccessFailedCount();

        // ✅ Performans iyileştirmesi: Bulk update kullanarak veriyi RAM'e çekmeden tek SQL sorgusuyla güncelle
        // Revoke existing active sessions for this device (if deviceId provided)
        // This ensures only one active session per device while allowing multi-device login
        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            await _context.RefreshSessions
                .Where(rs => rs.UserId == user.Id && 
                            rs.DeviceId == deviceId && 
                            !rs.Revoked && 
                            !rs.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(rs => rs.Revoked, true)
                    .SetProperty(rs => rs.RevokedAt, DateTime.UtcNow)
                    .SetProperty(rs => rs.RevokedReason, "Replaced by new login")
                    .SetProperty(rs => rs.UpdatedDate, DateTime.UtcNow)
                    .SetProperty(rs => rs.UpdatedById, SystemUsers.SystemUserId));
        }

        var authClaims = await _tokenService.GetAuthClaims(user);
        var accessToken = _tokenService.CreateAccessToken(authClaims, user);
        var refreshToken = _tokenService.CreateRefreshToken();

        var session = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Jti = accessToken.Jti,
            TokenHash = HashRefreshToken(refreshToken.Token),
            DeviceId = deviceId,
            ExpiresAt = refreshToken.ExpiresAt,
            Revoked = false,
            CreatedDate = DateTime.UtcNow,
            CreatedById = SystemUsers.SystemUserId
        };

        await _context.RefreshSessions.AddAsync(session);
        await SaveChangesWithConcurrencyHandlingAsync();

        var response = new AuthResult(
            user.Id,
            user.UserName,
            accessToken.ExpiresAt,
            accessToken.Token,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            accessToken.Permissions.ToList());

        return new SuccessDataResult<AuthResult>(response, "Giriş Başarılı");
    }

    public async Task<IDataResult<AuthResult>> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new AuthenticationErrorException("Geçersiz refresh token.");
        }

        var tokenHash = HashRefreshToken(refreshToken);
        var session = await _context.RefreshSessions
            .IgnoreQueryFilters() // includeDeleted: true için
            .FirstOrDefaultAsync(rs => rs.TokenHash == tokenHash);

        if (session is null)
        {
            throw new AuthenticationErrorException("Geçersiz refresh token.");
        }

        if (session.Revoked)
        {
            await RevokeAllSessionsAsync(session.UserId, "Replay detected");
            await SaveChangesWithConcurrencyHandlingAsync();
            throw new AuthenticationErrorException("Refresh token kullanılamaz durumda.");
        }

        if (session.ExpiresAt <= DateTime.UtcNow)
        {
            throw new AuthenticationErrorException("Refresh token süresi dolmuş.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == session.UserId && !u.IsDeleted)
            ?? throw new AuthenticationErrorException("Kullanıcı bulunamadı.");

        if (user.IsLockedOut())
        {
            throw new AuthenticationErrorException("Hesabınız kilitlenmiş.");
        }

        var claims = await _tokenService.GetAuthClaims(user);
        var newAccess = _tokenService.CreateAccessToken(claims, user);
        var newRefresh = _tokenService.CreateRefreshToken();

        var replacement = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Jti = newAccess.Jti,
            TokenHash = HashRefreshToken(newRefresh.Token),
            DeviceId = session.DeviceId,
            ExpiresAt = newRefresh.ExpiresAt,
            Revoked = false,
            CreatedDate = DateTime.UtcNow,
            CreatedById = SystemUsers.SystemUserId
        };

        session.Revoked = true;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = "Rotated";
        session.ReplacedById = replacement.Id;
        session.UpdatedDate = DateTime.UtcNow;
        session.UpdatedById = SystemUsers.SystemUserId;

        _context.RefreshSessions.Update(session);
        await _context.RefreshSessions.AddAsync(replacement);
        await SaveChangesWithConcurrencyHandlingAsync();

        var response = new AuthResult(
            user.Id,
            user.UserName,
            newAccess.ExpiresAt,
            newAccess.Token,
            newRefresh.Token,
            newRefresh.ExpiresAt,
            newAccess.Permissions.ToList());

        return new SuccessDataResult<AuthResult>(response, "Token yenilendi");
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = HashRefreshToken(refreshToken);
        var session = await _context.RefreshSessions
            .FirstOrDefaultAsync(rs => rs.TokenHash == tokenHash && !rs.IsDeleted);
        if (session is null)
        {
            return;
        }

        session.Revoked = true;
        session.RevokedAt = DateTime.UtcNow;
        session.RevokedReason = "Logout";
        session.UpdatedDate = DateTime.UtcNow;
        session.UpdatedById = SystemUsers.SystemUserId;

        _context.RefreshSessions.Update(session);
        await SaveChangesWithConcurrencyHandlingAsync();
    }

    public async Task PasswordResetAsync(string email)
    {
        User? user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        if (user != null)
        {
            // Rastgele token oluştur
            string resetToken = _passwordHasher.GeneratePasswordResetToken();

            // Token'ı hash'le ve veritabanına hash'i sakla
            string tokenHash = HashPasswordResetToken(resetToken);

            // ✅ SECURITY: Using User entity behavior method
            user.SetPasswordResetToken(tokenHash, DateTime.UtcNow.AddHours(1));

            _context.Users.Update(user);
            await SaveChangesWithConcurrencyHandlingAsync();

            // Kullanıcıya orijinal token'ı gönder (hash'i değil!)
            await _mailService.SendPasswordResetMailAsync(email, user.Id, resetToken.UrlEncode());
        }
    }

    private async Task SaveChangesWithConcurrencyHandlingAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Tekrarlanan refresh token isteklerinde oluşabilecek çakışmaları yakalayıp kullanıcıyı yeniden girişe yönlendiriyoruz.
            throw new AuthenticationErrorException("Oturum verileriniz başka bir işlem tarafından güncellendi. Lütfen tekrar giriş yapın.", ex);
        }
    }

    private async Task RevokeAllSessionsAsync(Guid userId, string reason)
    {
        // ✅ Performans iyileştirmesi: ExecuteUpdateAsync kullanarak veriyi RAM'e çekmeden tek SQL sorgusuyla güncelle
        await _context.RefreshSessions
            .Where(rs => rs.UserId == userId && !rs.Revoked && !rs.IsDeleted)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(rs => rs.Revoked, true)
                .SetProperty(rs => rs.RevokedAt, DateTime.UtcNow)
                .SetProperty(rs => rs.RevokedReason, reason)
                .SetProperty(rs => rs.UpdatedDate, DateTime.UtcNow)
                .SetProperty(rs => rs.UpdatedById, SystemUsers.SystemUserId));
    }

    public async Task<IDataResult<bool>> PasswordVerify(string resetToken, string userId)
    {
        try
        {
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                _logger.LogWarning("Invalid userId format provided for password verification");
                return new SuccessDataResult<bool>(false);
            }

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userIdGuid && !u.IsDeleted);
            if (user == null)
            {
                _logger.LogWarning("Password reset verification failed: User not found {UserId}", userIdGuid);
                return new SuccessDataResult<bool>(false);
            }

            if (user.PasswordResetToken == null || user.PasswordResetTokenExpiry == null)
            {
                _logger.LogWarning("Password reset verification failed: No reset token found for user {UserId}", userIdGuid);
                return new SuccessDataResult<bool>(false);
            }

            // ✅ Mantıksal hata düzeltmesi: Controller katmanında token zaten decode edilmiş olabilir
            // URL'den gelen token'lar genellikle otomatik decode edilir, body'den gelenler encode edilmiş olabilir
            // Try-catch ile decode işlemini yapıyoruz, hata olursa zaten decode edilmiş kabul ediyoruz
            try
            {
                // Eğer token encode edilmişse decode et
                if (!string.IsNullOrWhiteSpace(resetToken) && resetToken.Contains('-') == false)
                {
                    // Base64Url encoded token'lar genellikle '-' içermez, bu yüzden decode edilmeye çalışıyoruz
                    resetToken = resetToken.UrlDecode();
                }
            }
            catch
            {
                // Decode başarısız oldu, token zaten decode edilmiş kabul et
                _logger.LogDebug("Token decode edilemedi, zaten decode edilmiş kabul ediliyor");
            }

            string tokenHash = HashPasswordResetToken(resetToken);

            // ✅ SECURITY: Constant-time comparison to prevent timing attacks
            if (ConstantTimeEquals(user.PasswordResetToken, tokenHash) && user.PasswordResetTokenExpiry > DateTime.UtcNow)
            {
                return new SuccessDataResult<bool>(true);
            }

            _logger.LogWarning("Password reset verification failed: Invalid or expired token for user {UserId}", userIdGuid);
            return new SuccessDataResult<bool>(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset verification for userId: {UserId}", userId);
            return new SuccessDataResult<bool>(false);
        }
    }

    private static string HashRefreshToken(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    private static string HashPasswordResetToken(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Constant-time string comparison to prevent timing attacks
    /// </summary>
    private static bool ConstantTimeEquals(string? a, string? b)
    {
        if (a == null || b == null)
            return a == b;

        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}

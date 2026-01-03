using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace LifeOS.Application.Features.Music.ConnectMusic;

public sealed class ConnectMusicHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ConnectMusicHandler> _logger;

    public ConnectMusicHandler(
        LifeOSDbContext context,
        ISpotifyApiService spotifyApiService,
        ISpotifyTokenEncryptionService tokenEncryptionService,
        ICurrentUserService currentUserService,
        ILogger<ConnectMusicHandler> logger)
    {
        _context = context;
        _spotifyApiService = spotifyApiService;
        _tokenEncryptionService = tokenEncryptionService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResult<ConnectMusicResponse>> HandleAsync(
        ConnectMusicCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning("ConnectMusic: Kullanıcı kimliği bulunamadı");
            return ApiResultExtensions.Failure<ConnectMusicResponse>("Yetkisiz erişim");
        }

        try
        {
            _logger.LogInformation("ConnectMusic: Token exchange başlatılıyor. UserId: {UserId}", userId);

            // Authorization code ile token al
            var tokenResponse = await _spotifyApiService.ExchangeCodeForTokenAsync(command.Code, cancellationToken);

            _logger.LogInformation("ConnectMusic: Token alındı. User profile alınıyor. UserId: {UserId}", userId);

            // Kullanıcı profil bilgilerini al
            var userProfile = await _spotifyApiService.GetUserProfileAsync(tokenResponse.AccessToken, cancellationToken);

            _logger.LogInformation("ConnectMusic: User profile alındı. SpotifyUserId: {SpotifyUserId}, UserId: {UserId}", 
                userProfile.Id, userId);

            // Mevcut bağlantıyı kontrol et
            var existingConnection = await _context.MusicConnections
                .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted, cancellationToken);

            var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            if (existingConnection != null)
            {
                _logger.LogInformation("ConnectMusic: Mevcut bağlantı güncelleniyor. UserId: {UserId}", userId);
                // Mevcut bağlantıyı güncelle
                existingConnection.UpdateTokens(
                    _tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                    _tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                    expiresAt);
                existingConnection.Activate();
                _context.MusicConnections.Update(existingConnection);
            }
            else
            {
                _logger.LogInformation("ConnectMusic: Yeni bağlantı oluşturuluyor. UserId: {UserId}", userId);
                // Yeni bağlantı oluştur
                var connection = MusicConnection.Create(
                    userId.Value,
                    _tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                    _tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                    expiresAt,
                    userProfile.Id,
                    userProfile.DisplayName,
                    userProfile.Email);

                await _context.MusicConnections.AddAsync(connection, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("ConnectMusic: Bağlantı başarıyla kaydedildi. UserId: {UserId}", userId);

            return ApiResultExtensions.Success(
                new ConnectMusicResponse(true, "Spotify hesabı başarıyla bağlandı"),
                "Spotify hesabı başarıyla bağlandı");
        }
        catch (DbUpdateException ex)
        {
            // Entity Framework hatalarını detaylı logla
            var errorDetails = new StringBuilder();
            errorDetails.AppendLine($"DbUpdateException: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                errorDetails.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                errorDetails.AppendLine($"Inner Exception Type: {ex.InnerException.GetType().Name}");
                
                // PostgreSQL hataları için özel mesaj
                if (ex.InnerException.Message.Contains("duplicate key"))
                {
                    errorDetails.AppendLine("HATA: Aynı kayıt zaten mevcut (duplicate key)");
                }
                else if (ex.InnerException.Message.Contains("foreign key"))
                {
                    errorDetails.AppendLine("HATA: Foreign key constraint ihlali");
                }
                else if (ex.InnerException.Message.Contains("not null"))
                {
                    errorDetails.AppendLine("HATA: Zorunlu alan boş (not null constraint)");
                }
            }
            
            // Entry'leri logla
            if (ex.Entries != null && ex.Entries.Any())
            {
                errorDetails.AppendLine($"Etkilenen Entity Sayısı: {ex.Entries.Count()}");
                foreach (var entry in ex.Entries)
                {
                    errorDetails.AppendLine($"  - Entity Type: {entry.Entity.GetType().Name}, State: {entry.State}");
                }
            }
            
            var fullError = errorDetails.ToString();
            _logger.LogError(ex, "ConnectMusic: Database hatası. UserId: {UserId}\n{ErrorDetails}", userId, fullError);
            
            // Kullanıcıya daha anlaşılır mesaj döndür
            var userMessage = ex.InnerException?.Message ?? ex.Message;
            if (userMessage.Contains("duplicate key"))
            {
                userMessage = "Bu Spotify hesabı zaten bağlı. Lütfen önce bağlantıyı kesin.";
            }
            else if (userMessage.Contains("foreign key"))
            {
                userMessage = "Veritabanı hatası: İlişkili kayıt bulunamadı.";
            }
            else if (userMessage.Contains("not null"))
            {
                userMessage = "Veritabanı hatası: Zorunlu alan eksik.";
            }
            
            throw new InvalidOperationException($"Veritabanı hatası: {userMessage}", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "ConnectMusic: Spotify API hatası. UserId: {UserId}, StatusCode: {StatusCode}", 
                userId, ex.Data.Contains("StatusCode") ? ex.Data["StatusCode"] : "Unknown");
            throw; // Endpoint'te yakalanacak
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "ConnectMusic: İşlem hatası. UserId: {UserId}", userId);
            throw; // Endpoint'te yakalanacak
        }
        catch (Exception ex)
        {
            // Tüm exception'ların inner exception'ını logla
            var errorDetails = new StringBuilder();
            errorDetails.AppendLine($"=== EXCEPTION DETAILS ===");
            errorDetails.AppendLine($"Exception Type: {ex.GetType().FullName}");
            errorDetails.AppendLine($"Message: {ex.Message}");
            
            // Inner exception'ı recursive olarak logla
            Exception? currentEx = ex;
            int depth = 0;
            while (currentEx != null && depth < 5) // Maksimum 5 seviye derinlik
            {
                errorDetails.AppendLine($"--- Inner Exception Level {depth} ---");
                errorDetails.AppendLine($"Type: {currentEx.GetType().FullName}");
                errorDetails.AppendLine($"Message: {currentEx.Message}");
                
                // DbUpdateException için özel işlem
                if (currentEx is DbUpdateException dbEx)
                {
                    errorDetails.AppendLine($"DbUpdateException detected!");
                    if (dbEx.InnerException != null)
                    {
                        errorDetails.AppendLine($"  Inner Type: {dbEx.InnerException.GetType().FullName}");
                        errorDetails.AppendLine($"  Inner Message: {dbEx.InnerException.Message}");
                        
                        // PostgreSQL hataları için özel mesaj
                        if (dbEx.InnerException.Message.Contains("duplicate key"))
                        {
                            errorDetails.AppendLine("  HATA TİPİ: Duplicate key (aynı kayıt zaten mevcut)");
                        }
                        else if (dbEx.InnerException.Message.Contains("foreign key"))
                        {
                            errorDetails.AppendLine("  HATA TİPİ: Foreign key constraint ihlali");
                        }
                        else if (dbEx.InnerException.Message.Contains("not null"))
                        {
                            errorDetails.AppendLine("  HATA TİPİ: Not null constraint ihlali");
                        }
                    }
                    
                    if (dbEx.Entries != null && dbEx.Entries.Any())
                    {
                        errorDetails.AppendLine($"  Etkilenen Entity Sayısı: {dbEx.Entries.Count()}");
                        foreach (var entry in dbEx.Entries)
                        {
                            errorDetails.AppendLine($"    - Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                        }
                    }
                }
                
                currentEx = currentEx.InnerException;
                depth++;
            }
            
            errorDetails.AppendLine($"Stack Trace: {ex.StackTrace}");
            errorDetails.AppendLine($"=== END EXCEPTION DETAILS ===");
            
            var fullError = errorDetails.ToString();
            _logger.LogError(ex, "ConnectMusic: Beklenmeyen hata. UserId: {UserId}\n{ErrorDetails}", userId, fullError);
            throw; // Endpoint'te yakalanacak
        }
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            _logger.LogError(ex, "ConnectMusic: Beklenmeyen hata. UserId: {UserId}", userId);
            throw; // Endpoint'te yakalanacak
        }
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Application.Features.Music.GetMusicAuthorizationUrl;

public sealed class GetMusicAuthorizationUrlHandler
{
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetMusicAuthorizationUrlHandler> _logger;

    public GetMusicAuthorizationUrlHandler(
        ISpotifyApiService spotifyApiService,
        ICurrentUserService currentUserService,
        ILogger<GetMusicAuthorizationUrlHandler> logger)
    {
        _spotifyApiService = spotifyApiService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public ApiResult<GetMusicAuthorizationUrlResponse> Handle()
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GetMusicAuthorizationUrlResponse>("Yetkisiz erişim");
        }

        try
        {
            // State için güvenli bir token oluştur (CSRF koruması için)
            var state = GenerateSecureState(userId.Value);

            _logger.LogInformation("Spotify authorization URL oluşturuluyor. UserId: {UserId}", userId.Value);
            var authorizationUrl = _spotifyApiService.GetAuthorizationUrl(state);
            _logger.LogInformation("Spotify authorization URL başarıyla oluşturuldu");

            return ApiResultExtensions.Success(
                new GetMusicAuthorizationUrlResponse(authorizationUrl, state),
                "Authorization URL oluşturuldu");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Spotify configuration hatası");
            return ApiResultExtensions.Failure<GetMusicAuthorizationUrlResponse>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spotify authorization URL oluşturulurken hata");
            return ApiResultExtensions.Failure<GetMusicAuthorizationUrlResponse>($"Authorization URL oluşturulamadı: {ex.Message}");
        }
    }

    private static string GenerateSecureState(Guid userId)
    {
        // UserId + timestamp + random bytes ile güvenli state oluştur
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var data = $"{userId}:{timestamp}:{Convert.ToBase64String(randomBytes)}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }
}


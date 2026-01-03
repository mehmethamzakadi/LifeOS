using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.GetCurrentTrack;

public sealed class GetCurrentTrackHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentTrackHandler(
        LifeOSDbContext context,
        ISpotifyApiService spotifyApiService,
        ISpotifyTokenEncryptionService tokenEncryptionService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _spotifyApiService = spotifyApiService;
        _tokenEncryptionService = tokenEncryptionService;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<GetCurrentTrackResponse?>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GetCurrentTrackResponse?>("Yetkisiz erişim");
        }

        var connection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted && c.IsActive, cancellationToken);

        if (connection == null)
        {
            return ApiResultExtensions.Success<GetCurrentTrackResponse?>(null, "Spotify bağlantısı bulunamadı");
        }

        // Token'ı decrypt et
        var accessToken = _tokenEncryptionService.Decrypt(connection.AccessToken);

        // Token süresi dolmuşsa yenile
        if (connection.ExpiresAt <= DateTime.UtcNow)
        {
            try
            {
                var refreshToken = _tokenEncryptionService.Decrypt(connection.RefreshToken);
                var tokenResponse = await _spotifyApiService.RefreshTokenAsync(refreshToken, cancellationToken);
                
                var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                connection.UpdateTokens(
                    _tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                    _tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                    expiresAt);
                
                _context.MusicConnections.Update(connection);
                await _context.SaveChangesAsync(cancellationToken);
                
                accessToken = tokenResponse.AccessToken;
            }
            catch
            {
                return ApiResultExtensions.Failure<GetCurrentTrackResponse?>("Token yenilenemedi");
            }
        }

        try
        {
            var currentlyPlaying = await _spotifyApiService.GetCurrentlyPlayingAsync(accessToken, cancellationToken);
            
            if (currentlyPlaying == null || currentlyPlaying.Item == null)
            {
                return ApiResultExtensions.Success<GetCurrentTrackResponse?>(null, "Şu an hiçbir şarkı çalmıyor");
            }

            var track = currentlyPlaying.Item;
            var response = new GetCurrentTrackResponse(
                currentlyPlaying.IsPlaying,
                new CurrentTrackItem(
                    track.Id,
                    track.Name,
                    track.Artists.Select(a => new CurrentTrackArtist(a.Id, a.Name)).ToList(),
                    track.Album != null ? new CurrentTrackAlbum(
                        track.Album.Id,
                        track.Album.Name,
                        track.Album.Images.Select(i => new CurrentTrackImage(i.Url, i.Height, i.Width)).ToList()
                    ) : null,
                    track.DurationMs,
                    track.PreviewUrl,
                    track.ExternalUrl
                ),
                currentlyPlaying.ProgressMs,
                currentlyPlaying.Timestamp
            );

            return ApiResultExtensions.Success(response, "Şu an dinlenen şarkı getirildi");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<GetCurrentTrackResponse?>($"Şarkı bilgisi alınamadı: {ex.Message}");
        }
    }
}


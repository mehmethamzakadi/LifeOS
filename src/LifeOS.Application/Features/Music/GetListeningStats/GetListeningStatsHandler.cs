using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.GetListeningStats;

public sealed class GetListeningStatsHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    public GetListeningStatsHandler(
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

    public async Task<ApiResult<GetListeningStatsResponse>> HandleAsync(
        GetListeningStatsQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GetListeningStatsResponse>("Yetkisiz erişim");
        }

        var connection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted && c.IsActive, cancellationToken);

        if (connection == null)
        {
            return ApiResultExtensions.Failure<GetListeningStatsResponse>("Spotify bağlantısı bulunamadı");
        }

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
                return ApiResultExtensions.Failure<GetListeningStatsResponse>("Token yenilenemedi");
            }
        }

        try
        {
            // Spotify time range mapping
            var timeRange = query.Period switch
            {
                "daily" => "short_term", // Last 4 weeks
                "weekly" => "medium_term", // Last 6 months
                "monthly" => "long_term", // Last several years
                _ => "medium_term"
            };

            var topTracksTask = _spotifyApiService.GetTopTracksAsync(accessToken, timeRange, 20, cancellationToken);
            var topArtistsTask = _spotifyApiService.GetTopArtistsAsync(accessToken, timeRange, 20, cancellationToken);

            await Task.WhenAll(topTracksTask, topArtistsTask);

            var topTracks = await topTracksTask;
            var topArtists = await topArtistsTask;

            // Calculate total listening time from history
            var startDate = query.Period switch
            {
                "daily" => DateTime.UtcNow.AddDays(-1),
                "weekly" => DateTime.UtcNow.AddDays(-7),
                "monthly" => DateTime.UtcNow.AddDays(-30),
                _ => DateTime.UtcNow.AddDays(-7)
            };

            var totalListeningTime = await _context.MusicListeningHistory
                .Where(h => h.UserId == userId.Value && h.PlayedAt >= startDate)
                .SumAsync(h => h.ProgressMs ?? h.DurationMs, cancellationToken);

            // Get most listened genre
            var mostListenedGenre = await _context.MusicListeningHistory
                .Where(h => h.UserId == userId.Value && h.PlayedAt >= startDate && !string.IsNullOrEmpty(h.Genre))
                .GroupBy(h => h.Genre)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync(cancellationToken);

            var response = new GetListeningStatsResponse(
                topTracks.Items.Select(t => new TopTrackDto(
                    t.Id,
                    t.Name,
                    t.Artists.Select(a => new TopArtistDto(a.Id, a.Name)).ToList(),
                    t.Album != null ? new TopAlbumDto(
                        t.Album.Id,
                        t.Album.Name,
                        t.Album.Images.Select(i => new TopImageDto(i.Url, i.Height, i.Width)).ToList()
                    ) : null,
                    t.DurationMs,
                    t.PreviewUrl,
                    t.ExternalUrl
                )).ToList(),
                topArtists.Items.Select(a => new TopArtistDto(a.Id, a.Name)).ToList(),
                totalListeningTime,
                mostListenedGenre
            );

            return ApiResultExtensions.Success(response, "İstatistikler getirildi");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<GetListeningStatsResponse>($"İstatistikler alınamadı: {ex.Message}");
        }
    }
}


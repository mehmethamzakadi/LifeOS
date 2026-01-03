using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.GenerateMoodPlaylist;

public sealed class GenerateMoodPlaylistHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    // Ruh hali → Basit Audio Features mapping (sadece target değerleri)
    private static readonly Dictionary<string, (double TargetValence, double TargetEnergy)> MoodMappings = new()
    {
        ["mutlu"] = (0.8, 0.7),      // Yüksek pozitiflik, orta-yüksek enerji
        ["üzgün"] = (0.3, 0.4),      // Düşük pozitiflik, düşük enerji
        ["enerjik"] = (0.7, 0.8),    // Yüksek pozitiflik, yüksek enerji
        ["sakin"] = (0.6, 0.3),      // Orta pozitiflik, düşük enerji
        ["romantik"] = (0.6, 0.4),   // Orta pozitiflik, düşük enerji
        ["nostaljik"] = (0.55, 0.5)  // Orta pozitiflik, orta enerji
    };

    // Dil tercihi → Market/Genre mapping
    private static readonly Dictionary<string, (string? Market, List<string> Genres)> LanguageMappings = new()
    {
        ["turkish"] = ("TR", new List<string> { "turkish-pop", "turkish-rock" }),
        ["foreign"] = ("US", new List<string> { "pop", "rock", "electronic" }),
        ["mixed"] = (null, new List<string> { "pop", "rock", "turkish-pop" })
    };

    public GenerateMoodPlaylistHandler(
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

    public async Task<ApiResult<GenerateMoodPlaylistResponse>> HandleAsync(
        GenerateMoodPlaylistCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>("Yetkisiz erişim");
        }

        // Ruh hali kontrolü
        if (!MoodMappings.ContainsKey(command.Mood.ToLowerInvariant()))
        {
            return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>(
                $"Geçersiz ruh hali. Desteklenenler: {string.Join(", ", MoodMappings.Keys)}");
        }

        var connection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted && c.IsActive, cancellationToken);

        if (connection == null)
        {
            return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>("Spotify bağlantısı bulunamadı");
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
                return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>("Token yenilenemedi");
            }
        }

        try
        {
            var mood = command.Mood.ToLowerInvariant();
            var (targetValence, targetEnergy) = MoodMappings[mood];
            var (market, genres) = LanguageMappings.GetValueOrDefault(
                command.LanguagePreference.ToLowerInvariant(),
                LanguageMappings["mixed"]);

            // 1. Kullanıcının top tracks'lerinden seed seç (3-5 şarkı)
            var topTracks = await _spotifyApiService.GetTopTracksAsync(
                accessToken, "medium_term", 20, cancellationToken);

            var seedTracks = topTracks.Items
                .Take(5)
                .Select(t => t.Id)
                .ToList();

            // 2. Basit açıklama (AI kullanmadan)
            var moodDescriptions = new Dictionary<string, string>
            {
                ["mutlu"] = "Mutlu anlarınız için özel olarak hazırlanmış neşeli şarkılar.",
                ["üzgün"] = "Hüzünlü ruh halinize eşlik edecek sakin ve duygusal şarkılar.",
                ["enerjik"] = "Enerjinizi yükseltecek, hareketli ve dinamik şarkılar.",
                ["sakin"] = "Sakinleşmek ve rahatlamak için huzur verici şarkılar.",
                ["romantik"] = "Romantik anlarınız için özel olarak seçilmiş şarkılar.",
                ["nostaljik"] = "Geçmişe yolculuk yapmanızı sağlayacak nostaljik şarkılar."
            };
            var description = moodDescriptions.GetValueOrDefault(mood, $"{mood} ruh haline özel playlist.");

            // 3. Spotify Recommendations API çağrısı (basit ve direkt)
            var recommendations = await _spotifyApiService.GetRecommendationsAsync(
                accessToken,
                seedTracks: seedTracks.Any() ? seedTracks : null,
                seedArtists: null,
                seedGenres: genres,
                targetValence: targetValence,
                targetEnergy: targetEnergy,
                targetDanceability: null,
                minTempo: null,
                maxTempo: null,
                market: market,
                limit: Math.Clamp(command.Limit, 10, 50),
                cancellationToken);

            var tracks = recommendations.Tracks.Select(t => new PlaylistTrackDto(
                t.Id,
                t.Name,
                t.Artists.Select(a => new PlaylistArtistDto(a.Id, a.Name)).ToList(),
                t.Album != null ? new PlaylistAlbumDto(
                    t.Album.Id,
                    t.Album.Name,
                    t.Album.Images.Select(i => new PlaylistImageDto(i.Url, i.Height, i.Width)).ToList()
                ) : null,
                t.DurationMs,
                t.PreviewUrl,
                t.ExternalUrl
            )).ToList();

            var response = new GenerateMoodPlaylistResponse(
                tracks,
                mood,
                description
            );

            return ApiResultExtensions.Success(response, "Playlist başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>(
                $"Playlist oluşturulamadı: {ex.Message}");
        }
    }
}


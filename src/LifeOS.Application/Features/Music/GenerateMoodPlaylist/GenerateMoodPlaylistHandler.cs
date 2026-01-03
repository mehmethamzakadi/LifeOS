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
    // ÖNEMLİ: Spotify API'de "turkish-pop" gibi birleşik genre isimleri geçersizdir
    // Geçerli genre'ler: "turkish", "pop", "rock", "electronic" vb. (ayrı ayrı)
    private static readonly Dictionary<string, (string? Market, List<string> Genres)> LanguageMappings = new()
    {
        ["turkish"] = ("TR", new List<string> { "turkish", "pop" }), // "turkish-pop" yerine "turkish" ve "pop" ayrı
        ["foreign"] = ("US", new List<string> { "pop", "rock", "electronic" }),
        ["mixed"] = (null, new List<string> { "pop", "rock", "turkish" })
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
        var mood = command.Mood.ToLowerInvariant();
        if (!MoodMappings.TryGetValue(mood, out var moodTargets))
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
                return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>("Token yenilenemedi. Lütfen tekrar giriş yapın.");
            }
        }

        try
        {
            var (market, availableGenres) = LanguageMappings.GetValueOrDefault(
                command.LanguagePreference.ToLowerInvariant(),
                LanguageMappings["mixed"]);

            // 1. Kullanıcının top tracks'lerini al
            var topTracksResponse = await _spotifyApiService.GetTopTracksAsync(
                accessToken, "medium_term", 10, cancellationToken); // 20 yerine 10 yeterli

            // 2. Seed Seçim Mantığı (Toplamda tam 5 adet olacak şekilde)
            // Spotify kuralı: seed_artists + seed_genres + seed_tracks <= 5
            List<string> seedTracks = new();
            List<string> seedGenres = new();

            if (topTracksResponse.Items != null && topTracksResponse.Items.Any())
            {
                // En fazla 3 track alalım ki genre'lara yer kalsın
                seedTracks = topTracksResponse.Items
                    .Take(3)
                    .Select(t => t.Id)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList();
            }

            // Kalan kontenjanı genre ile doldur (Max 5 - track sayısı)
            int remainingSlots = 5 - seedTracks.Count;
            if (remainingSlots > 0)
            {
                seedGenres = availableGenres.Take(remainingSlots).ToList();
            }

            // Eğer hiç seed yoksa fallback genre ata
            if (!seedTracks.Any() && !seedGenres.Any())
            {
                seedGenres = new List<string> { "pop" }; // Fallback
            }

            // 3. Market parametresi
            // Eğer seed_tracks kullanıyorsak market parametresini 'null' göndermek bazen daha güvenlidir
            // çünkü seed track o markette yoksa hata verir. Spotify'ın user marketini kullanmasına izin veririz.
            string? targetMarket = seedTracks.Any() ? null : market;

            // 4. Spotify Recommendations API çağrısı
            var recommendations = await _spotifyApiService.GetRecommendationsAsync(
                accessToken,
                seedTracks: seedTracks.Any() ? seedTracks : null,
                seedArtists: null, // Artist yerine Track/Genre karışımı daha iyi sonuç verir
                seedGenres: seedGenres.Any() ? seedGenres : null,
                targetValence: moodTargets.TargetValence,
                targetEnergy: moodTargets.TargetEnergy,
                targetDanceability: null,
                minTempo: null,
                maxTempo: null,
                market: targetMarket,
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

            // Basit açıklama
            var description = $"{mood} modunda, {(command.LanguagePreference == "turkish" ? "Türkçe" : command.LanguagePreference == "foreign" ? "yabancı" : "karışık")} parçalar.";

            var response = new GenerateMoodPlaylistResponse(
                tracks,
                mood,
                description
            );

            return ApiResultExtensions.Success(response, "Playlist önerileri başarıyla oluşturuldu");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<GenerateMoodPlaylistResponse>(
                $"Spotify hatası: {ex.Message}");
        }
    }
}


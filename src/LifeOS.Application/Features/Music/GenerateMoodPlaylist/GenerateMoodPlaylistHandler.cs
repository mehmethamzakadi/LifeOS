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
    private readonly IAiService _aiService;

    // Ruh hali → Audio Features mapping
    private static readonly Dictionary<string, MoodAudioFeatures> MoodMappings = new()
    {
        ["mutlu"] = new(MinValence: 0.7, TargetValence: 0.8, MinEnergy: 0.6, TargetEnergy: 0.7, MinDanceability: 0.6),
        ["üzgün"] = new(MaxValence: 0.4, TargetValence: 0.3, MaxEnergy: 0.5, TargetEnergy: 0.4, MaxTempo: 100),
        ["enerjik"] = new(MinEnergy: 0.7, TargetEnergy: 0.8, MinDanceability: 0.6, MinTempo: 120, TargetTempo: 130),
        ["sakin"] = new(MaxEnergy: 0.4, TargetEnergy: 0.3, MinValence: 0.5, TargetValence: 0.6, MaxTempo: 100),
        ["romantik"] = new(MinValence: 0.5, MaxValence: 0.7, TargetValence: 0.6, MaxEnergy: 0.5, TargetEnergy: 0.4, MinTempo: 60, MaxTempo: 90),
        ["nostaljik"] = new(MinValence: 0.4, MaxValence: 0.7, TargetValence: 0.55, MaxEnergy: 0.6, TargetEnergy: 0.5)
    };

    // Dil tercihi → Market/Genre mapping
    private static readonly Dictionary<string, (string? Market, List<string> Genres)> LanguageMappings = new()
    {
        ["turkish"] = ("TR", new List<string> { "turkish-pop", "turkish-rock", "turkish-hip-hop" }),
        ["foreign"] = ("US", new List<string> { "pop", "rock", "electronic", "hip-hop" }),
        ["mixed"] = (null, new List<string> { "pop", "rock", "turkish-pop" })
    };

    private sealed record MoodAudioFeatures(
        double? MinValence = null,
        double? MaxValence = null,
        double? TargetValence = null,
        double? MinEnergy = null,
        double? MaxEnergy = null,
        double? TargetEnergy = null,
        double? MinDanceability = null,
        double? TargetDanceability = null,
        double? MinTempo = null,
        double? MaxTempo = null,
        double? TargetTempo = null
    );

    public GenerateMoodPlaylistHandler(
        LifeOSDbContext context,
        ISpotifyApiService spotifyApiService,
        ISpotifyTokenEncryptionService tokenEncryptionService,
        ICurrentUserService currentUserService,
        IAiService aiService)
    {
        _context = context;
        _spotifyApiService = spotifyApiService;
        _tokenEncryptionService = tokenEncryptionService;
        _currentUserService = currentUserService;
        _aiService = aiService;
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
            var moodFeatures = MoodMappings[mood];
            var languageMapping = LanguageMappings.GetValueOrDefault(
                command.LanguagePreference.ToLowerInvariant(),
                LanguageMappings["mixed"]);

            // 1. Kullanıcının top tracks'lerinden seed seç (orta ağırlıkta)
            var topTracks = await _spotifyApiService.GetTopTracksAsync(
                accessToken, "medium_term", 20, cancellationToken);

            // Seed tracks seçimi: Ruh haline uygun şarkıları filtrele
            var seedTracks = new List<string>();
            if (topTracks.Items.Any())
            {
                // Top tracks'lerden audio features al
                var trackIds = topTracks.Items.Take(10).Select(t => t.Id).ToList();
                var audioFeatures = await _spotifyApiService.GetAudioFeaturesAsync(
                    accessToken, trackIds, cancellationToken);

                // Ruh haline uygun şarkıları seç
                var matchingTracks = audioFeatures
                    .Where(af => MatchesMood(af, moodFeatures))
                    .Select(af => af.Id)
                    .Take(3)
                    .ToList();

                seedTracks.AddRange(matchingTracks);

                // Eğer yeterli seed yoksa, rastgele ekle
                if (seedTracks.Count < 2 && topTracks.Items.Any())
                {
                    var remaining = topTracks.Items
                        .Where(t => !seedTracks.Contains(t.Id))
                        .Take(2 - seedTracks.Count)
                        .Select(t => t.Id);
                    seedTracks.AddRange(remaining);
                }
            }

            // 2. AI ile ruh halini detaylandır (opsiyonel, performans için skip edilebilir)
            string description;
            try
            {
                var aiPrompt = $"{mood} ruh haline uygun bir müzik playlist'i için kısa bir açıklama yaz (maksimum 2 cümle, Türkçe).";
                description = await _aiService.GenerateCategoryDescriptionAsync(aiPrompt, cancellationToken);
            }
            catch
            {
                // AI başarısız olursa varsayılan açıklama
                description = $"{mood} ruh haline özel olarak hazırlanmış kişiselleştirilmiş playlist.";
            }

            // 3. Spotify Recommendations API çağrısı
            var recommendations = await _spotifyApiService.GetRecommendationsAsync(
                accessToken,
                seedTracks: seedTracks.Any() ? seedTracks : null,
                seedArtists: null,
                seedGenres: languageMapping.Genres,
                targetValence: moodFeatures.TargetValence,
                targetEnergy: moodFeatures.TargetEnergy,
                targetDanceability: moodFeatures.TargetDanceability,
                minTempo: moodFeatures.MinTempo,
                maxTempo: moodFeatures.MaxTempo,
                market: languageMapping.Market,
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

    private static bool MatchesMood(
        SpotifyAudioFeaturesResponse features,
        MoodAudioFeatures moodFeatures)
    {
        if (moodFeatures.MinValence.HasValue && features.Valence < moodFeatures.MinValence.Value)
            return false;
        if (moodFeatures.MaxValence.HasValue && features.Valence > moodFeatures.MaxValence.Value)
            return false;
        if (moodFeatures.MinEnergy.HasValue && features.Energy < moodFeatures.MinEnergy.Value)
            return false;
        if (moodFeatures.MaxEnergy.HasValue && features.Energy > moodFeatures.MaxEnergy.Value)
            return false;
        if (moodFeatures.MinDanceability.HasValue && features.Danceability < moodFeatures.MinDanceability.Value)
            return false;
        if (moodFeatures.MinTempo.HasValue && features.Tempo < moodFeatures.MinTempo.Value)
            return false;
        if (moodFeatures.MaxTempo.HasValue && features.Tempo > moodFeatures.MaxTempo.Value)
            return false;

        return true;
    }
}


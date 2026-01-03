using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LifeOS.Application.Features.Music.AnalyzeArtist;

public sealed class AnalyzeArtistHandler
{
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ILogger<AnalyzeArtistHandler> _logger;

    public AnalyzeArtistHandler(
        ISpotifyApiService spotifyApiService,
        ILogger<AnalyzeArtistHandler> logger)
    {
        _spotifyApiService = spotifyApiService;
        _logger = logger;
    }

    public async Task<ApiResult<AnalyzeArtistResponse>> HandleAsync(
        AnalyzeArtistQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Client Credentials Flow ile token al (kullanıcı login gerekmez)
            var accessToken = await _spotifyApiService.GetClientCredentialsTokenAsync(cancellationToken);

            // 2. Sanatçı ara
            // Sanatçı adını temizle ve boşlukları kontrol et
            var artistName = query.ArtistName?.Trim();
            if (string.IsNullOrWhiteSpace(artistName))
            {
                return ApiResultExtensions.Failure<AnalyzeArtistResponse>(
                    "Sanatçı adı boş olamaz.");
            }

            var searchResult = await _spotifyApiService.SearchArtistsAsync(
                accessToken, 
                artistName, 
                limit: 1, 
                cancellationToken);

            if (searchResult.Artists.Items == null || !searchResult.Artists.Items.Any())
            {
                return ApiResultExtensions.Failure<AnalyzeArtistResponse>(
                    $"'{query.ArtistName}' adında bir sanatçı bulunamadı.");
            }

            var artist = searchResult.Artists.Items.First();
            var artistId = artist.Id;

            // 3. Sanatçının en popüler 5 şarkısını al
            var topTracks = await _spotifyApiService.GetArtistTopTracksAsync(
                accessToken,
                artistId,
                market: "TR",
                cancellationToken);

            if (topTracks.Items == null || !topTracks.Items.Any())
            {
                return ApiResultExtensions.Failure<AnalyzeArtistResponse>(
                    $"'{artist.Name}' için popüler şarkı bulunamadı.");
            }

            // En fazla 5 şarkı al
            var tracks = topTracks.Items.Take(5).ToList();

            // 4. Şarkıların audio features'larını al (opsiyonel - Client Credentials Flow ile erişim kısıtlı olabilir)
            var trackIds = tracks
                .Where(t => !string.IsNullOrWhiteSpace(t.Id))
                .Select(t => t.Id)
                .ToList();

            if (!trackIds.Any())
            {
                return ApiResultExtensions.Failure<AnalyzeArtistResponse>(
                    "Analiz edilebilir şarkı bulunamadı.");
            }

            List<SpotifyAudioFeaturesResponse> audioFeatures = new();
            bool audioFeaturesAvailable = false;

            try
            {
                _logger.LogInformation("Audio features çekiliyor: {TrackCount} şarkı için, TrackIds: {TrackIds}", 
                    trackIds.Count, string.Join(", ", trackIds));

                audioFeatures = await _spotifyApiService.GetAudioFeaturesAsync(
                    accessToken,
                    trackIds,
                    cancellationToken);

                _logger.LogInformation("Audio features alındı: {FeatureCount} özellik bulundu", audioFeatures.Count);
                audioFeaturesAvailable = audioFeatures.Any();
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("Forbidden") || ex.Message.Contains("403") || ex.Message.Contains("Unauthorized") || ex.Message.Contains("401"))
            {
                // 403 Forbidden veya 401 Unauthorized hatası - Client Credentials Flow ile Audio Features API'sine erişim kısıtlı olabilir
                _logger.LogWarning("Audio Features API'sine erişim kısıtlı (Client Credentials Flow): {Message}. Sadece şarkı bilgileri gösterilecek.", ex.Message);
                audioFeaturesAvailable = false;
            }
            catch (Exception ex)
            {
                // Diğer hatalar için de devam et, sadece şarkı bilgilerini göster
                _logger.LogWarning(ex, "Audio Features alınırken hata oluştu. Sadece şarkı bilgileri gösterilecek.");
                audioFeaturesAvailable = false;
            }

            // 5. Sonuçları birleştir
            var trackAnalyses = new List<TrackAnalysis>();
            foreach (var track in tracks)
            {
                var features = audioFeatures.FirstOrDefault(f => f.Id == track.Id);
                bool hasFeatures = features != null && audioFeaturesAvailable;

                string? valenceDescription = null;
                if (hasFeatures && features != null)
                {
                    valenceDescription = features.Valence > 0.65 
                        ? "Mutlu" 
                        : features.Valence > 0.35 
                            ? "Dengeli" 
                            : "Hüzünlü";
                }

                trackAnalyses.Add(new TrackAnalysis
                {
                    Id = track.Id,
                    Name = track.Name,
                    ArtistName = track.Artists.FirstOrDefault()?.Name ?? artist.Name,
                    Valence = hasFeatures ? features?.Valence : null,
                    Energy = hasFeatures ? features?.Energy : null,
                    Danceability = hasFeatures ? features?.Danceability : null,
                    Tempo = hasFeatures ? features?.Tempo : null,
                    ValenceDescription = valenceDescription,
                    HasAudioFeatures = hasFeatures
                });
            }

            if (!trackAnalyses.Any())
            {
                return ApiResultExtensions.Failure<AnalyzeArtistResponse>(
                    "Şarkı bilgileri alınamadı.");
            }

            var response = new AnalyzeArtistResponse
            {
                ArtistName = artist.Name,
                ArtistId = artist.Id,
                ArtistImageUrl = artist.Images.FirstOrDefault()?.Url,
                Tracks = trackAnalyses
            };

            return ApiResultExtensions.Success(response, "Sanatçı analizi başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sanatçı analizi hatası: {Message}", ex.Message);
            return ApiResultExtensions.Failure<AnalyzeArtistResponse>(
                $"Sanatçı analizi yapılamadı: {ex.Message}");
        }
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.AnalyzeVibe;

public sealed class AnalyzeVibeHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    public AnalyzeVibeHandler(
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

    public async Task<ApiResult<AnalyzeVibeResponse>> HandleAsync(
        AnalyzeVibeQuery query,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<AnalyzeVibeResponse>("Yetkisiz erişim");
        }

        var connection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted && c.IsActive, cancellationToken);

        if (connection == null)
        {
            return ApiResultExtensions.Failure<AnalyzeVibeResponse>("Spotify bağlantısı bulunamadı");
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
                return ApiResultExtensions.Failure<AnalyzeVibeResponse>("Token yenilenemedi. Lütfen tekrar giriş yapın.");
            }
        }

        try
        {
            // 1. Kullanıcının son zamanlarda en çok dinlediği şarkıları al
            var topTracks = await _spotifyApiService.GetTopTracksAsync(
                accessToken, query.TimeRange, 20, cancellationToken);

            if (topTracks.Items == null || !topTracks.Items.Any())
            {
                return ApiResultExtensions.Success(
                    new AnalyzeVibeResponse(
                        "Henüz yeterli veri yok",
                        "📊",
                        0,
                        0,
                        0,
                        null,
                        0
                    ),
                    "Henüz yeterli dinleme verisi yok. Biraz daha müzik dinleyin!");
            }

            var trackIds = topTracks.Items
                .Where(t => !string.IsNullOrWhiteSpace(t.Id))
                .Select(t => t.Id)
                .ToList();

            if (!trackIds.Any())
            {
                return ApiResultExtensions.Failure<AnalyzeVibeResponse>("Şarkı ID'leri alınamadı");
            }

            // 2. Bu şarkıların Audio Features verilerini al
            var audioFeatures = await _spotifyApiService.GetAudioFeaturesAsync(
                accessToken, trackIds, cancellationToken);

            if (!audioFeatures.Any())
            {
                return ApiResultExtensions.Failure<AnalyzeVibeResponse>("Audio features alınamadı");
            }

            // 3. Ortalamaları hesapla
            double avgEnergy = audioFeatures.Average(x => x.Energy);
            double avgValence = audioFeatures.Average(x => x.Valence); // Valence = Pozitiflik/Mutluluk
            double avgDanceability = audioFeatures.Average(x => x.Danceability);

            // 4. Ruh hali analizi (Business Logic)
            string vibeDescription;
            string moodIcon;

            if (avgEnergy > 0.7 && avgValence > 0.6)
            {
                vibeDescription = "Ateş ediyorsun! Enerjin ve keyfin çok yerinde.";
                moodIcon = "🔥";
            }
            else if (avgEnergy < 0.4 && avgValence < 0.4)
            {
                vibeDescription = "Biraz melankolik ve durgun bir dönem.";
                moodIcon = "🌧️";
            }
            else if (avgEnergy > 0.6 && avgValence < 0.4)
            {
                vibeDescription = "Gergin veya öfkelisin. Müzikler sert ama hüzünlü.";
                moodIcon = "⚡";
            }
            else if (avgEnergy < 0.4 && avgValence > 0.6)
            {
                vibeDescription = "Sakin ve huzurlu bir ruh halindesin.";
                moodIcon = "☕";
            }
            else if (avgValence > 0.7)
            {
                vibeDescription = "Çok mutlu ve neşeli bir dönem!";
                moodIcon = "😊";
            }
            else
            {
                vibeDescription = "Dengeli, sakin bir akıştasın.";
                moodIcon = "🌊";
            }

            // 5. En çok dinlenen sanatçı (basit bir yaklaşım)
            var topArtist = topTracks.Items
                .SelectMany(t => t.Artists)
                .GroupBy(a => a.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            var response = new AnalyzeVibeResponse(
                MoodTitle: vibeDescription,
                MoodIcon: moodIcon,
                EnergyLevel: (int)(avgEnergy * 100),
                HappinessLevel: (int)(avgValence * 100),
                DanceabilityLevel: (int)(avgDanceability * 100),
                TopGenre: topArtist ?? "Bilinmeyen",
                AnalyzedTracksCount: audioFeatures.Count
            );

            return ApiResultExtensions.Success(response, "Ruh hali analizi tamamlandı");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<AnalyzeVibeResponse>(
                $"Analiz hatası: {ex.Message}");
        }
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Infrastructure.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeOS.Infrastructure.Services.BackgroundServices;

/// <summary>
/// Spotify dinleme geçmişini periyodik olarak senkronize eden background service.
/// Aktif Spotify bağlantıları olan tüm kullanıcılar için son dinlenen şarkıları çeker ve veritabanına kaydeder.
/// </summary>
public class MusicListeningHistorySyncService : BackgroundService
{
    private readonly ILogger<MusicListeningHistorySyncService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(30); // Her 30 dakikada bir senkronize et

    public MusicListeningHistorySyncService(
        ILogger<MusicListeningHistorySyncService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Music Listening History Sync Service başlatıldı. Senkronizasyon aralığı: {Interval} dakika", SyncInterval.TotalMinutes);

        // İlk çalışmadan önce kısa bir gecikme (uygulama başlangıcında hemen çalışmasın)
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncListeningHistoryForAllUsersAsync(stoppingToken);
                await Task.Delay(SyncInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Music Listening History Sync Service durduruluyor...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dinleme geçmişi senkronizasyonu sırasında hata oluştu");
                // Hata durumunda bir sonraki denemeye kadar bekle
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task SyncListeningHistoryForAllUsersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LifeOSDbContext>();
            var spotifyApiService = scope.ServiceProvider.GetRequiredService<ISpotifyApiService>();
            var tokenEncryptionService = scope.ServiceProvider.GetRequiredService<ISpotifyTokenEncryptionService>();
            var executionContextAccessor = scope.ServiceProvider.GetRequiredService<IExecutionContextAccessor>();

            using var auditScope = executionContextAccessor.BeginScope(SystemUsers.SystemUserId);

            // Aktif Spotify bağlantıları olan tüm kullanıcıları bul
            var activeConnections = await context.MusicConnections
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync(cancellationToken);

            if (!activeConnections.Any())
            {
                _logger.LogDebug("Senkronize edilecek aktif Spotify bağlantısı bulunamadı");
                return;
            }

            _logger.LogInformation("{Count} aktif Spotify bağlantısı için dinleme geçmişi senkronizasyonu başlatılıyor", activeConnections.Count);

            var totalSynced = 0;
            var totalErrors = 0;

            foreach (var connection in activeConnections)
            {
                try
                {
                    var syncedCount = await SyncListeningHistoryForUserAsync(
                        context,
                        connection,
                        spotifyApiService,
                        tokenEncryptionService,
                        cancellationToken);

                    totalSynced += syncedCount;
                }
                catch (Exception ex)
                {
                    totalErrors++;
                    _logger.LogWarning(ex, "Kullanıcı {UserId} için dinleme geçmişi senkronizasyonu başarısız oldu", connection.UserId);
                }
            }

            _logger.LogInformation(
                "Dinleme geçmişi senkronizasyonu tamamlandı. {Synced} yeni kayıt eklendi, {Errors} hata oluştu",
                totalSynced,
                totalErrors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dinleme geçmişi senkronizasyonu başarısız oldu");
        }
    }

    private async Task<int> SyncListeningHistoryForUserAsync(
        LifeOSDbContext context,
        MusicConnection connection,
        ISpotifyApiService spotifyApiService,
        ISpotifyTokenEncryptionService tokenEncryptionService,
        CancellationToken cancellationToken)
    {
        // Token'ı decrypt et
        var accessToken = tokenEncryptionService.Decrypt(connection.AccessToken);

        // Token süresi dolmuşsa yenile
        if (connection.ExpiresAt <= DateTime.UtcNow)
        {
            try
            {
                var refreshToken = tokenEncryptionService.Decrypt(connection.RefreshToken);
                var tokenResponse = await spotifyApiService.RefreshTokenAsync(refreshToken, cancellationToken);

                var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                connection.UpdateTokens(
                    tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                    tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                    expiresAt);

                context.MusicConnections.Update(connection);
                await context.SaveChangesAsync(cancellationToken);

                accessToken = tokenResponse.AccessToken;
                _logger.LogDebug("Kullanıcı {UserId} için Spotify token yenilendi", connection.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kullanıcı {UserId} için token yenilenemedi", connection.UserId);
                throw;
            }
        }

        // Son senkronizasyon zamanından sonraki dinleme geçmişini çek
        // Eğer ilk senkronizasyon ise, son 24 saatlik geçmişi çek
        var since = connection.LastSyncedAt ?? DateTime.UtcNow.AddHours(-24);

        // Spotify API'den son dinlenen şarkıları çek (limit: 50)
        var recentlyPlayed = await spotifyApiService.GetRecentlyPlayedAsync(accessToken, limit: 50, cancellationToken);

        if (!recentlyPlayed.Items.Any())
        {
            _logger.LogDebug("Kullanıcı {UserId} için yeni dinleme geçmişi bulunamadı", connection.UserId);
            connection.UpdateSyncTime();
            context.MusicConnections.Update(connection);
            await context.SaveChangesAsync(cancellationToken);
            return 0;
        }

        // Veritabanında zaten kayıtlı olan track'leri kontrol et (duplicate önleme)
        var existingTrackIds = await context.MusicListeningHistory
            .Where(h => h.UserId == connection.UserId && h.PlayedAt >= since)
            .Select(h => h.SpotifyTrackId)
            .ToListAsync(cancellationToken);

        var newHistoryItems = new List<MusicListeningHistory>();
        var syncedCount = 0;

        foreach (var item in recentlyPlayed.Items)
        {
            // Sadece son senkronizasyon zamanından sonraki kayıtları al
            // item.PlayedAt zaten DateTime olarak geliyor (SpotifyApiService'de çevriliyor)
            var playedAt = item.PlayedAt;

            if (playedAt < since)
            {
                continue; // Bu kayıt zaten senkronize edilmiş
            }

            // Duplicate kontrolü
            if (existingTrackIds.Contains(item.Track.Id) && 
                await context.MusicListeningHistory
                    .AnyAsync(h => h.UserId == connection.UserId && 
                                 h.SpotifyTrackId == item.Track.Id && 
                                 Math.Abs((h.PlayedAt - playedAt).TotalSeconds) < 5, // 5 saniye tolerans
                                 cancellationToken))
            {
                continue; // Bu kayıt zaten var
            }

            // Yeni dinleme geçmişi kaydı oluştur
            var historyItem = MusicListeningHistory.Create(
                userId: connection.UserId,
                spotifyTrackId: item.Track.Id,
                trackName: item.Track.Name,
                artistName: item.Track.Artists.FirstOrDefault()?.Name ?? "Unknown Artist",
                albumName: item.Track.Album?.Name,
                genre: null, // Genre bilgisi track detaylarından alınabilir (şimdilik null)
                playedAt: playedAt,
                durationMs: item.Track.DurationMs,
                progressMs: null // Recently played API'si progress bilgisi vermiyor
            );

            newHistoryItems.Add(historyItem);
            syncedCount++;
        }

        if (newHistoryItems.Any())
        {
            await context.MusicListeningHistory.AddRangeAsync(newHistoryItems, cancellationToken);
            connection.UpdateSyncTime();
            context.MusicConnections.Update(connection);
            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Kullanıcı {UserId} için {Count} yeni dinleme geçmişi kaydı eklendi",
                connection.UserId,
                syncedCount);
        }
        else
        {
            // Yeni kayıt yoksa sadece sync zamanını güncelle
            connection.UpdateSyncTime();
            context.MusicConnections.Update(connection);
            await context.SaveChangesAsync(cancellationToken);
        }

        return syncedCount;
    }
}


using LifeOS.Application.Features.Music.GetCurrentTrack;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeOS.Infrastructure.Services.BackgroundServices;

/// <summary>
/// Şu an çalan şarkıyı periyodik olarak kontrol edip SignalR ile real-time broadcast eden background service.
/// Her 5 saniyede bir aktif Spotify bağlantıları olan kullanıcılar için current track'i kontrol eder ve günceller.
/// </summary>
public class MusicCurrentTrackBroadcastService : BackgroundService
{
    private readonly ILogger<MusicCurrentTrackBroadcastService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(5); // Her 5 saniyede bir kontrol et

    public MusicCurrentTrackBroadcastService(
        ILogger<MusicCurrentTrackBroadcastService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Music Current Track Broadcast Service başlatıldı. Kontrol aralığı: {Interval} saniye", CheckInterval.TotalSeconds);

        // İlk çalışmadan önce kısa bir gecikme
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastCurrentTrackForAllUsersAsync(stoppingToken);
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Music Current Track Broadcast Service durduruluyor...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Current track broadcast sırasında hata oluştu");
                // Hata durumunda bir sonraki denemeye kadar bekle
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task BroadcastCurrentTrackForAllUsersAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LifeOSDbContext>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<LifeOS.API.Hubs.MusicHub>>();

            // Aktif Spotify bağlantıları olan tüm kullanıcıları bul
            var activeConnections = await context.MusicConnections
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync(cancellationToken);

            if (!activeConnections.Any())
            {
                return; // Aktif bağlantı yoksa log spam'ini önle
            }

            // Her kullanıcı için current track'i kontrol et ve broadcast et
            foreach (var connection in activeConnections)
            {
                try
                {
                    // GetCurrentTrackHandler'ı kullanmak için ICurrentUserService'i geçici olarak set etmemiz gerekiyor
                    // Bunun yerine direkt SpotifyApiService kullanabiliriz veya handler'ı çağırabiliriz
                    // Şimdilik handler'ı kullanmayacağız, direkt API service kullanacağız
                    await BroadcastCurrentTrackForUserAsync(
                        context,
                        hubContext,
                        connection,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kullanıcı {UserId} için current track broadcast başarısız oldu", connection.UserId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Current track broadcast başarısız oldu");
        }
    }

    private async Task BroadcastCurrentTrackForUserAsync(
        LifeOSDbContext context,
        IHubContext<LifeOS.API.Hubs.MusicHub> hubContext,
        MusicConnection connection,
        CancellationToken cancellationToken)
    {
        var tokenEncryptionService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ISpotifyTokenEncryptionService>();
        var spotifyApiService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ISpotifyApiService>();

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
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kullanıcı {UserId} için token yenilenemedi", connection.UserId);
                return; // Token yenilenemezse broadcast yapma
            }
        }

        try
        {
            // Spotify'dan current track'i al
            var currentlyPlaying = await spotifyApiService.GetCurrentlyPlayingAsync(accessToken, cancellationToken);

            // Response DTO oluştur
            GetCurrentTrackResponse? response = null;
            if (currentlyPlaying != null && currentlyPlaying.Item != null)
            {
                var track = currentlyPlaying.Item;
                response = new GetCurrentTrackResponse(
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
            }

            // SignalR ile kullanıcıya broadcast et
            var userId = connection.UserId.ToString();
            await hubContext.Clients.Group($"user_{userId}").SendAsync("CurrentTrackUpdated", response, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kullanıcı {UserId} için current track alınamadı", connection.UserId);
        }
    }
}


using LifeOS.Domain.Common;
using LifeOS.Domain.Constants;
using LifeOS.Infrastructure.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LifeOS.Infrastructure.Services.BackgroundServices;

/// <summary>
/// Süresi dolmuş veya iptal edilmiş refresh session kayıtlarını periyodik olarak temizler.
/// Performance ve veritabanı boyutunu optimize etmek için gereklidir.
/// </summary>
public class SessionCleanupService : BackgroundService
{
    private readonly ILogger<SessionCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _cleanupInterval = BackgroundServiceConstants.SessionCleanupInterval;

    public SessionCleanupService(
        ILogger<SessionCleanupService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Cleanup Service başlatıldı. Temizlik aralığı: {Interval} saat", _cleanupInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
                await CleanupExpiredSessionsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Session Cleanup Service durduruluyor...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Session temizliği sırasında hata oluştu");
            }
        }
    }

    private async Task CleanupExpiredSessionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LifeOSDbContext>();
            var executionContextAccessor = scope.ServiceProvider.GetRequiredService<IExecutionContextAccessor>();

            using var auditScope = executionContextAccessor.BeginScope(SystemUsers.SystemUserId);

            // Soft delete expired sessions (süresi dolmuş ve revoked olmayan session'ları sil)
            var deletedCount = await context.RefreshSessions
                .Where(rs => (rs.ExpiresAt <= DateTime.UtcNow || rs.Revoked) && !rs.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(rs => rs.IsDeleted, true)
                    .SetProperty(rs => rs.DeletedDate, DateTime.UtcNow),
                    cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Süresi dolmuş {Count} refresh session kaydı temizlendi", deletedCount);
            }
            else
            {
                _logger.LogDebug("Temizlenecek süresi dolmuş session bulunamadı");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session cleanup işlemi başarısız oldu");
        }
    }
}

using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

public interface IRefreshSessionRepository : IRepository<RefreshSession>
{
    Task<RefreshSession?> GetByTokenHashAsync(string tokenHash, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update: Tüm aktif session'ları iptal et (ExecuteUpdateAsync kullanarak - performans için)
    /// </summary>
    Task<int> RevokeAllActiveSessionsAsync(Guid userId, string reason, Guid updatedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update: Belirli device'a ait aktif session'ları iptal et (ExecuteUpdateAsync kullanarak - performans için)
    /// </summary>
    Task<int> RevokeActiveSessionsByDeviceAsync(Guid userId, string deviceId, string reason, Guid updatedById, CancellationToken cancellationToken = default);
}

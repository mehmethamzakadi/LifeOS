using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

public sealed class RefreshSessionRepository : EfRepositoryBase<RefreshSession, LifeOSDbContext>, IRefreshSessionRepository
{
    public RefreshSessionRepository(LifeOSDbContext context) : base(context)
    {
    }

    public async Task<RefreshSession?> GetByTokenHashAsync(string tokenHash, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        IQueryable<RefreshSession> query = Context.RefreshSessions.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.RefreshSessions
            .Where(x => x.UserId == userId && !x.Revoked && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default)
    {
        // Süresi dolmuş veya iptal edilmiş session'ları sil (30 gün üzeri)
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        return await Context.RefreshSessions
            .IgnoreQueryFilters()
            .Where(x =>
                (x.Revoked && x.RevokedAt < cutoffDate) ||
                (!x.Revoked && x.ExpiresAt < cutoffDate))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<int> RevokeAllActiveSessionsAsync(Guid userId, string reason, Guid updatedById, CancellationToken cancellationToken = default)
    {
        // ✅ Performans iyileştirmesi: ExecuteUpdateAsync kullanarak veriyi RAM'e çekmeden tek SQL sorgusuyla güncelle
        var now = DateTime.UtcNow;
        return await Context.RefreshSessions
            .Where(x => x.UserId == userId && !x.Revoked && x.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Revoked, true)
                .SetProperty(x => x.RevokedAt, now)
                .SetProperty(x => x.RevokedReason, reason)
                .SetProperty(x => x.UpdatedDate, now)
                .SetProperty(x => x.UpdatedById, updatedById),
                cancellationToken);
    }

    public async Task<int> RevokeActiveSessionsByDeviceAsync(Guid userId, string deviceId, string reason, Guid updatedById, CancellationToken cancellationToken = default)
    {
        // ✅ Performans iyileştirmesi: ExecuteUpdateAsync kullanarak veriyi RAM'e çekmeden tek SQL sorgusuyla güncelle
        var now = DateTime.UtcNow;
        return await Context.RefreshSessions
            .Where(x => x.UserId == userId && x.DeviceId == deviceId && !x.Revoked && x.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Revoked, true)
                .SetProperty(x => x.RevokedAt, now)
                .SetProperty(x => x.RevokedReason, reason)
                .SetProperty(x => x.UpdatedDate, now)
                .SetProperty(x => x.UpdatedById, updatedById),
                cancellationToken);
    }
}

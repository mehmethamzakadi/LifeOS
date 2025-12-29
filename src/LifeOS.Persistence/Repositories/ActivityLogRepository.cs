using LifeOS.Domain.Common.Dynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;


namespace LifeOS.Persistence.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly LifeOSDbContext _context;

    public ActivityLogRepository(LifeOSDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Activity log ekler
    /// NOT: SaveChanges UnitOfWork tarafından yönetilir
    /// </summary>
    public async Task<ActivityLog> AddAsync(ActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        await _context.ActivityLogs.AddAsync(activityLog, cancellationToken);
        // ✅ SaveChangesAsync KALDIRILDI - UnitOfWork yönetecek
        return activityLog;
    }

    public async Task<List<ActivityLog>> GetRecentActivitiesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        return await _context.ActivityLogs
            .AsNoTracking()
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<Paginate<ActivityLog>> GetPaginatedListByDynamicAsync(
        DynamicQuery dynamic,
        int index = 0,
        int size = 10,
        Func<IQueryable<ActivityLog>, IQueryable<ActivityLog>>? include = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        IQueryable<ActivityLog> queryable = _context.ActivityLogs
            .AsNoTracking()
            .AsQueryable();

        if (include != null)
            queryable = include(queryable);

        // Apply dynamic filtering and sorting using ToDynamic extension
        queryable = queryable.ToDynamic(dynamic);

        var count = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip(index * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new Paginate<ActivityLog>
        {
            Items = items,
            Index = index,
            Size = size,
            Count = count,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
    }

    /// <summary>
    /// Idempotency kontrolü için - belirli bir ID'ye sahip ActivityLog var mı?
    /// </summary>
    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ActivityLogs
            .AnyAsync(a => a.Id == id, cancellationToken);
    }
}

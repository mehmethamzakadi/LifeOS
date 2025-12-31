using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WatchPlatforms.DeleteWatchPlatform;

public sealed class DeleteWatchPlatformHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteWatchPlatformHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var platform = await _context.WatchPlatforms
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (platform is null)
            return ApiResultExtensions.Failure("İzleme platformu bulunamadı");

        // Kullanımda mı kontrol et
        var isInUse = await _context.MovieSeries
            .AnyAsync(x => x.WatchPlatformId == id && !x.IsDeleted, cancellationToken);

        if (isInUse)
            return ApiResultExtensions.Failure("Bu platform kullanımda olduğu için silinemez");

        platform.Delete();
        _context.WatchPlatforms.Update(platform);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Add(
            CacheKeys.WatchPlatformListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("İzleme platformu başarıyla silindi");
    }
}


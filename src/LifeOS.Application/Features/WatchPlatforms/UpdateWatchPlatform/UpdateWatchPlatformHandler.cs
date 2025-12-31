using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WatchPlatforms.UpdateWatchPlatform;

public sealed class UpdateWatchPlatformHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateWatchPlatformHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        UpdateWatchPlatformCommand command,
        CancellationToken cancellationToken)
    {
        var platform = await _context.WatchPlatforms
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (platform is null)
            return ApiResultExtensions.Failure("İzleme platformu bulunamadı");

        // Aynı isimde başka bir platform var mı kontrol et
        bool nameExists = await _context.WatchPlatforms
            .AnyAsync(x => x.Id != command.Id && x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (nameExists)
            return ApiResultExtensions.Failure("Bu platform adı zaten kullanılıyor");

        platform.Update(command.Name);
        _context.WatchPlatforms.Update(platform);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.WatchPlatformListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("İzleme platformu başarıyla güncellendi");
    }
}


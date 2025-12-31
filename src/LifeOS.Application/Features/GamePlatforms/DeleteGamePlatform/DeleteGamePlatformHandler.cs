using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GamePlatforms.DeleteGamePlatform;

public sealed class DeleteGamePlatformHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteGamePlatformHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var platform = await _context.GamePlatforms
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (platform is null)
            return ApiResultExtensions.Failure("Oyun platformu bulunamadı");

        // Kullanımda mı kontrol et
        var isInUse = await _context.Games
            .AnyAsync(x => x.GamePlatformId == id && !x.IsDeleted, cancellationToken);

        if (isInUse)
            return ApiResultExtensions.Failure("Bu platform kullanımda olduğu için silinemez");

        platform.Delete();
        _context.GamePlatforms.Update(platform);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Add(
            CacheKeys.GamePlatformListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("Oyun platformu başarıyla silindi");
    }
}


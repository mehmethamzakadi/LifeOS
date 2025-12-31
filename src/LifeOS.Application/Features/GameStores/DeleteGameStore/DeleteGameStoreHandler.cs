using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GameStores.DeleteGameStore;

public sealed class DeleteGameStoreHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteGameStoreHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var store = await _context.GameStores
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (store is null)
            return ApiResultExtensions.Failure("Oyun mağazası bulunamadı");

        // Kullanımda mı kontrol et
        var isInUse = await _context.Games
            .AnyAsync(x => x.GameStoreId == id && !x.IsDeleted, cancellationToken);

        if (isInUse)
            return ApiResultExtensions.Failure("Bu mağaza kullanımda olduğu için silinemez");

        store.Delete();
        _context.GameStores.Update(store);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Add(
            CacheKeys.GameStoreListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("Oyun mağazası başarıyla silindi");
    }
}


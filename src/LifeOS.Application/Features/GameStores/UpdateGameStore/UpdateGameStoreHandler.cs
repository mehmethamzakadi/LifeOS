using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GameStores.UpdateGameStore;

public sealed class UpdateGameStoreHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateGameStoreHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        UpdateGameStoreCommand command,
        CancellationToken cancellationToken)
    {
        var store = await _context.GameStores
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (store is null)
            return ApiResultExtensions.Failure("Oyun mağazası bulunamadı");

        // Aynı isimde başka bir mağaza var mı kontrol et
        bool nameExists = await _context.GameStores
            .AnyAsync(x => x.Id != command.Id && x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (nameExists)
            return ApiResultExtensions.Failure("Bu mağaza adı zaten kullanılıyor");

        store.Update(command.Name);
        _context.GameStores.Update(store);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.GameStoreListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("Oyun mağazası başarıyla güncellendi");
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GameStores.CreateGameStore;

public sealed class CreateGameStoreHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateGameStoreHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateGameStoreResponse> HandleAsync(
        CreateGameStoreCommand command,
        CancellationToken cancellationToken)
    {
        bool storeExists = await _context.GameStores
            .AnyAsync(x => x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (storeExists)
        {
            throw new InvalidOperationException("Bu mağaza adı zaten mevcut!");
        }

        var store = GameStore.Create(command.Name);
        await _context.GameStores.AddAsync(store, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.GameStoreListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateGameStoreResponse(store.Id);
    }
}


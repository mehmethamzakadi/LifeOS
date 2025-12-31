using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WatchPlatforms.CreateWatchPlatform;

public sealed class CreateWatchPlatformHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateWatchPlatformHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateWatchPlatformResponse> HandleAsync(
        CreateWatchPlatformCommand command,
        CancellationToken cancellationToken)
    {
        bool platformExists = await _context.WatchPlatforms
            .AnyAsync(x => x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (platformExists)
        {
            throw new InvalidOperationException("Bu platform adı zaten mevcut!");
        }

        var platform = WatchPlatform.Create(command.Name);
        await _context.WatchPlatforms.AddAsync(platform, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.WatchPlatformListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateWatchPlatformResponse(platform.Id);
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GamePlatforms.CreateGamePlatform;

public sealed class CreateGamePlatformHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateGamePlatformHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateGamePlatformResponse> HandleAsync(
        CreateGamePlatformCommand command,
        CancellationToken cancellationToken)
    {
        bool platformExists = await _context.GamePlatforms
            .AnyAsync(x => x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (platformExists)
        {
            throw new InvalidOperationException("Bu platform adı zaten mevcut!");
        }

        var platform = GamePlatform.Create(command.Name);
        await _context.GamePlatforms.AddAsync(platform, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.GamePlatformListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateGamePlatformResponse(platform.Id);
    }
}


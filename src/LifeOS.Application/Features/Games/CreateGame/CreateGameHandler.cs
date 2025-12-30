using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Features.Games.GetGameById;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;

namespace LifeOS.Application.Features.Games.CreateGame;

public sealed class CreateGameHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateGameHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateGameResponse> HandleAsync(
        CreateGameCommand command,
        CancellationToken cancellationToken)
    {
        var game = Game.Create(
            command.Title,
            command.CoverUrl,
            command.Platform,
            command.Store,
            command.Status,
            command.IsOwned);

        await _context.Games.AddAsync(game, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.Game(game.Id),
            new GetGameByIdResponse(
                game.Id,
                game.Title,
                game.CoverUrl,
                game.Platform,
                game.Store,
                game.Status,
                game.IsOwned),
            DateTimeOffset.UtcNow.Add(CacheDurations.Game),
            null);

        await _cache.Add(
            CacheKeys.GameGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateGameResponse(game.Id);
    }
}


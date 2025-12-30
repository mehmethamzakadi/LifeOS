using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.Games.GetGameById;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.UpdateGame;

public sealed class UpdateGameHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateGameHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateGameCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var game = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (game is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.Game.NotFound);
        }

        game.Update(
            command.Title,
            command.CoverUrl,
            command.Platform,
            command.Store,
            command.Status,
            command.IsOwned);

        _context.Games.Update(game);
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

        return ApiResultExtensions.Success(ResponseMessages.Game.Updated);
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.DeleteGame;

public sealed class DeleteGameHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteGameHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (game is null)
            return ApiResultExtensions.Failure(ResponseMessages.Game.NotFound);

        game.Delete();
        _context.Games.Update(game);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Remove(CacheKeys.Game(game.Id));

        await _cacheService.Add(
            CacheKeys.GameGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.Game.Deleted);
    }
}


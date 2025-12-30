using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.GetGameById;

public sealed class GetGameByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public GetGameByIdHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<GetGameByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Game(id);
        var cacheValue = await _cacheService.Get<GetGameByIdResponse>(cacheKey);
        if (cacheValue is not null)
            return ApiResultExtensions.Success(cacheValue, "Oyun bilgisi başarıyla getirildi");

        var game = await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (game is null)
            return ApiResultExtensions.Failure<GetGameByIdResponse>("Oyun bilgisi bulunamadı.");

        var response = new GetGameByIdResponse(
            game.Id,
            game.Title,
            game.CoverUrl,
            game.Platform,
            game.Store,
            game.Status,
            game.IsOwned);

        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.Game),
            null);

        return ApiResultExtensions.Success(response, "Oyun bilgisi başarıyla getirildi");
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.Queries.GetById;

public sealed class GetGameByIdQueryHandler(
    LifeOSDbContext context,
    ICacheService cacheService) : IRequestHandler<GetByIdGameQuery, IDataResult<GetByIdGameResponse>>
{
    public async Task<IDataResult<GetByIdGameResponse>> Handle(GetByIdGameQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Game(request.Id);
        var cacheValue = await cacheService.Get<GetByIdGameResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdGameResponse>(cacheValue);

        var game = await context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (game is null)
            return new ErrorDataResult<GetByIdGameResponse>("Oyun bilgisi bulunamadı.");

        var response = new GetByIdGameResponse(
            game.Id,
            game.Title,
            game.CoverUrl,
            game.Platform,
            game.Store,
            game.Status,
            game.IsOwned);

        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.Game),
            null);

        return new SuccessDataResult<GetByIdGameResponse>(response);
    }
}


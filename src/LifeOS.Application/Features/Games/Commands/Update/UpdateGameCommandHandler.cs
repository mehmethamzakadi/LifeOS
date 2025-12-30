using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Games.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Games.Commands.Update;

public sealed class UpdateGameCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateGameCommand, IResult>
{
    public async Task<IResult> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
    {
        var game = await context.Games
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (game is null)
        {
            return new ErrorResult(ResponseMessages.Game.NotFound);
        }

        game.Update(
            request.Title,
            request.CoverUrl,
            request.Platform,
            request.Store,
            request.Status,
            request.IsOwned);

        context.Games.Update(game);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Add(
            CacheKeys.Game(game.Id),
            new GetByIdGameResponse(
                game.Id,
                game.Title,
                game.CoverUrl,
                game.Platform,
                game.Store,
                game.Status,
                game.IsOwned),
            DateTimeOffset.UtcNow.Add(CacheDurations.Game),
            null);

        await cacheService.Add(
            CacheKeys.GameGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Game.Updated);
    }
}


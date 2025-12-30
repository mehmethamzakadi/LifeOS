using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Games.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Games.Commands.Create;

public sealed class CreateGameCommandHandler(
    LifeOSDbContext context,
    ICacheService cache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateGameCommand, IResult>
{
    public async Task<IResult> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        var game = Game.Create(
            request.Title,
            request.CoverUrl,
            request.Platform,
            request.Store,
            request.Status,
            request.IsOwned);

        await context.Games.AddAsync(game, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.Add(
            CacheKeys.Game(game.Id),
            new GetByIdGameResponse(
                Id: game.Id,
                Title: game.Title,
                CoverUrl: game.CoverUrl,
                Platform: game.Platform,
                Store: game.Store,
                Status: game.Status,
                IsOwned: game.IsOwned),
            DateTimeOffset.UtcNow.Add(CacheDurations.Game),
            null);

        await cache.Add(
            CacheKeys.GameGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Game.Created);
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Games.Commands.Delete;

public sealed class DeleteGameCommandHandler(
    IGameRepository gameRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteGameCommand, IResult>
{
    public async Task<IResult> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetAsync(predicate: x => x.Id == request.Id, enableTracking: true, cancellationToken: cancellationToken);
        if (game is null)
            return new ErrorResult(ResponseMessages.Game.NotFound);

        game.Delete();
        gameRepository.Update(game);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.Game(game.Id));

        await cacheService.Add(
            CacheKeys.GameGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Game.Deleted);
    }
}


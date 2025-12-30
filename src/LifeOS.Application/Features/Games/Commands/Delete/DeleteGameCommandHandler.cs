using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Games.Commands.Delete;

public sealed class DeleteGameCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteGameCommand, IResult>
{
    public async Task<IResult> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
    {
        var game = await context.Games
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (game is null)
            return new ErrorResult(ResponseMessages.Game.NotFound);

        game.Delete();
        context.Games.Update(game);
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


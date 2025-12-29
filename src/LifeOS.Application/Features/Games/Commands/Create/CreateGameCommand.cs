using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Enums;
using MediatR;

namespace LifeOS.Application.Features.Games.Commands.Create;

public sealed record CreateGameCommand(
    string Title,
    string? CoverUrl = null,
    GamePlatform Platform = GamePlatform.PC,
    GameStore Store = GameStore.Steam,
    GameStatus Status = GameStatus.Backlog,
    bool IsOwned = false) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.GameGridVersion();
    }
}


using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Games.Commands.Delete;

public sealed record DeleteGameCommand(Guid Id) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.Game(Id);
        yield return CacheKeys.GameGridVersion();
    }
}


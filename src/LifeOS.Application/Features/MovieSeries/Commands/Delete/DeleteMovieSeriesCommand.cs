using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.MovieSeries.Commands.Delete;

public sealed record DeleteMovieSeriesCommand(Guid Id) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.MovieSeries(Id);
        yield return CacheKeys.MovieSeriesGridVersion();
    }
}


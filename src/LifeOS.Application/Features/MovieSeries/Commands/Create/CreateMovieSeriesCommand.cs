using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Enums;
using MediatR;

namespace LifeOS.Application.Features.MovieSeries.Commands.Create;

public sealed record CreateMovieSeriesCommand(
    string Title,
    string? CoverUrl = null,
    MovieSeriesType Type = MovieSeriesType.Movie,
    MovieSeriesPlatform Platform = MovieSeriesPlatform.Netflix,
    int? CurrentSeason = null,
    int? CurrentEpisode = null,
    MovieSeriesStatus Status = MovieSeriesStatus.ToWatch,
    int? Rating = null,
    string? PersonalNote = null) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.MovieSeriesGridVersion();
    }
}


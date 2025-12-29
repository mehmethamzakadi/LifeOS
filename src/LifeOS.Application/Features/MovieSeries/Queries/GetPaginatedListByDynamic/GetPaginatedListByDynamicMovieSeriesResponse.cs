using LifeOS.Application.Common;
using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicMovieSeriesResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public MovieSeriesType Type { get; init; }
    public MovieSeriesPlatform Platform { get; init; }
    public int? CurrentSeason { get; init; }
    public int? CurrentEpisode { get; init; }
    public MovieSeriesStatus Status { get; init; }
    public int? Rating { get; init; }
    public string? PersonalNote { get; init; }
}


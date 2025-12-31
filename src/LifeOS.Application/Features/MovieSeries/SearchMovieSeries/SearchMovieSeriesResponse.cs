using LifeOS.Application.Common;
using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.MovieSeries.SearchMovieSeries;

public sealed record SearchMovieSeriesResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public Guid MovieSeriesGenreId { get; init; }
    public string GenreName { get; init; } = string.Empty;
    public Guid WatchPlatformId { get; init; }
    public string WatchPlatformName { get; init; } = string.Empty;
    public int? CurrentSeason { get; init; }
    public int? CurrentEpisode { get; init; }
    public MovieSeriesStatus Status { get; init; }
    public int? Rating { get; init; }
    public string? PersonalNote { get; init; }
}


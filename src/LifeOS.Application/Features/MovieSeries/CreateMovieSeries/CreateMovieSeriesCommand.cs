using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.MovieSeries.CreateMovieSeries;

public sealed record CreateMovieSeriesCommand(
    string Title,
    string? CoverUrl = null,
    Guid MovieSeriesGenreId = default,
    Guid WatchPlatformId = default,
    int? CurrentSeason = null,
    int? CurrentEpisode = null,
    MovieSeriesStatus Status = MovieSeriesStatus.ToWatch,
    int? Rating = null,
    string? PersonalNote = null);


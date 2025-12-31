using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.MovieSeries.UpdateMovieSeries;

public sealed record UpdateMovieSeriesCommand(
    Guid Id,
    string Title,
    string? CoverUrl = null,
    Guid GenreId = default,
    Guid WatchPlatformId = default,
    int? CurrentSeason = null,
    int? CurrentEpisode = null,
    MovieSeriesStatus Status = MovieSeriesStatus.ToWatch,
    int? Rating = null,
    string? PersonalNote = null);


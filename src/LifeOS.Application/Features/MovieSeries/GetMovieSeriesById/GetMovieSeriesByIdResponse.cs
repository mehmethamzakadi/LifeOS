using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;

public sealed record GetMovieSeriesByIdResponse(
    Guid Id,
    string Title,
    string? CoverUrl,
    Guid GenreId,
    string GenreName,
    Guid WatchPlatformId,
    string WatchPlatformName,
    int? CurrentSeason,
    int? CurrentEpisode,
    MovieSeriesStatus Status,
    int? Rating,
    string? PersonalNote);


namespace LifeOS.Application.Features.MovieSeriesGenres.UpdateMovieSeriesGenre;

public sealed record UpdateMovieSeriesGenreCommand(
    Guid Id,
    string Name);


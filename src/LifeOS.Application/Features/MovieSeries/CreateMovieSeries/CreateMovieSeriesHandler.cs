using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;

namespace LifeOS.Application.Features.MovieSeries.CreateMovieSeries;

public sealed class CreateMovieSeriesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateMovieSeriesHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateMovieSeriesResponse> HandleAsync(
        CreateMovieSeriesCommand command,
        CancellationToken cancellationToken)
    {
        var movieSeries = MovieSeriesEntity.Create(
            command.Title,
            command.CoverUrl,
            command.GenreId,
            command.WatchPlatformId,
            command.CurrentSeason,
            command.CurrentEpisode,
            command.Status,
            command.Rating,
            command.PersonalNote);

        await _context.MovieSeries.AddAsync(movieSeries, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation - MovieSeries entity'yi yeniden yükle
        await _context.Entry(movieSeries).Reference(m => m.Genre).LoadAsync(cancellationToken);
        await _context.Entry(movieSeries).Reference(m => m.WatchPlatform).LoadAsync(cancellationToken);

        await _cache.Add(
            CacheKeys.MovieSeries(movieSeries.Id),
            new GetMovieSeriesByIdResponse(
                movieSeries.Id,
                movieSeries.Title,
                movieSeries.CoverUrl,
                movieSeries.GenreId,
                movieSeries.Genre.Name,
                movieSeries.WatchPlatformId,
                movieSeries.WatchPlatform.Name,
                movieSeries.CurrentSeason,
                movieSeries.CurrentEpisode,
                movieSeries.Status,
                movieSeries.Rating,
                movieSeries.PersonalNote),
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
            null);

        await _cache.Add(
            CacheKeys.MovieSeriesGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateMovieSeriesResponse(movieSeries.Id);
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.UpdateMovieSeries;

public sealed class UpdateMovieSeriesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateMovieSeriesHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateMovieSeriesCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var movieSeries = await _context.MovieSeries
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (movieSeries is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.MovieSeries.NotFound);
        }

        movieSeries.Update(
            command.Title,
            command.CoverUrl,
            command.MovieSeriesGenreId,
            command.WatchPlatformId,
            command.CurrentSeason,
            command.CurrentEpisode,
            command.Status,
            command.Rating,
            command.PersonalNote);

        _context.MovieSeries.Update(movieSeries);
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
                movieSeries.MovieSeriesGenreId,
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

        return ApiResultExtensions.Success(ResponseMessages.MovieSeries.Updated);
    }
}


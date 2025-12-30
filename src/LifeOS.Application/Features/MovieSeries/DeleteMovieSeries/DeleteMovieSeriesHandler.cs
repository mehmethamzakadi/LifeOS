using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.DeleteMovieSeries;

public sealed class DeleteMovieSeriesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteMovieSeriesHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var movieSeries = await _context.MovieSeries
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (movieSeries is null)
            return ApiResultExtensions.Failure(ResponseMessages.MovieSeries.NotFound);

        movieSeries.Delete();
        _context.MovieSeries.Update(movieSeries);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Remove(CacheKeys.MovieSeries(movieSeries.Id));

        await _cacheService.Add(
            CacheKeys.MovieSeriesGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.MovieSeries.Deleted);
    }
}


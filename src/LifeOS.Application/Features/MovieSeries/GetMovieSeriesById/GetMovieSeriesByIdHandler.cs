using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;

public sealed class GetMovieSeriesByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public GetMovieSeriesByIdHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<GetMovieSeriesByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.MovieSeries(id);
        var cacheValue = await _cacheService.Get<GetMovieSeriesByIdResponse>(cacheKey);
        if (cacheValue is not null)
            return ApiResultExtensions.Success(cacheValue, "Film/Dizi bilgisi başarıyla getirildi");

        var movieSeries = await _context.MovieSeries
            .Include(m => m.Genre)
            .Include(m => m.WatchPlatform)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (movieSeries is null)
            return ApiResultExtensions.Failure<GetMovieSeriesByIdResponse>("Film/Dizi bilgisi bulunamadı.");

        var response = new GetMovieSeriesByIdResponse(
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
            movieSeries.PersonalNote);

        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
            null);

        return ApiResultExtensions.Success(response, "Film/Dizi bilgisi başarıyla getirildi");
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetById;

public sealed class GetMovieSeriesByIdQueryHandler(
    LifeOSDbContext context,
    ICacheService cacheService) : IRequestHandler<GetByIdMovieSeriesQuery, IDataResult<GetByIdMovieSeriesResponse>>
{
    public async Task<IDataResult<GetByIdMovieSeriesResponse>> Handle(GetByIdMovieSeriesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.MovieSeries(request.Id);
        var cacheValue = await cacheService.Get<GetByIdMovieSeriesResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdMovieSeriesResponse>(cacheValue);

        var movieSeries = await context.MovieSeries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (movieSeries is null)
            return new ErrorDataResult<GetByIdMovieSeriesResponse>("Film/Dizi bilgisi bulunamadı.");

        var response = new GetByIdMovieSeriesResponse(
            movieSeries.Id,
            movieSeries.Title,
            movieSeries.CoverUrl,
            movieSeries.Type,
            movieSeries.Platform,
            movieSeries.CurrentSeason,
            movieSeries.CurrentEpisode,
            movieSeries.Status,
            movieSeries.Rating,
            movieSeries.PersonalNote);

        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
            null);

        return new SuccessDataResult<GetByIdMovieSeriesResponse>(response);
    }
}


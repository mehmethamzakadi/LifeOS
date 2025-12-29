using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetById;

public sealed class GetMovieSeriesByIdQueryHandler(
    IMovieSeriesRepository movieSeriesRepository,
    ICacheService cacheService) : IRequestHandler<GetByIdMovieSeriesQuery, IDataResult<GetByIdMovieSeriesResponse>>
{
    public async Task<IDataResult<GetByIdMovieSeriesResponse>> Handle(GetByIdMovieSeriesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.MovieSeries(request.Id);
        var cacheValue = await cacheService.Get<GetByIdMovieSeriesResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdMovieSeriesResponse>(cacheValue);

        var movieSeries = await movieSeriesRepository.GetByIdAsync(request.Id, cancellationToken);

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


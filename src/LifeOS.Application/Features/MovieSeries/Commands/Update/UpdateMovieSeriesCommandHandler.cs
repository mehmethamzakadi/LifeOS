using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.MovieSeries.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.MovieSeries.Commands.Update;

public sealed class UpdateMovieSeriesCommandHandler(
    IMovieSeriesRepository movieSeriesRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMovieSeriesCommand, IResult>
{
    public async Task<IResult> Handle(UpdateMovieSeriesCommand request, CancellationToken cancellationToken)
    {
        var movieSeries = await movieSeriesRepository.GetAsync(
            predicate: x => x.Id == request.Id,
            enableTracking: true,
            cancellationToken: cancellationToken);

        if (movieSeries is null)
        {
            return new ErrorResult(ResponseMessages.MovieSeries.NotFound);
        }

        movieSeries.Update(
            request.Title,
            request.CoverUrl,
            request.Type,
            request.Platform,
            request.CurrentSeason,
            request.CurrentEpisode,
            request.Status,
            request.Rating,
            request.PersonalNote);

        movieSeriesRepository.Update(movieSeries);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Add(
            CacheKeys.MovieSeries(movieSeries.Id),
            new GetByIdMovieSeriesResponse(
                movieSeries.Id,
                movieSeries.Title,
                movieSeries.CoverUrl,
                movieSeries.Type,
                movieSeries.Platform,
                movieSeries.CurrentSeason,
                movieSeries.CurrentEpisode,
                movieSeries.Status,
                movieSeries.Rating,
                movieSeries.PersonalNote),
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
            null);

        await cacheService.Add(
            CacheKeys.MovieSeriesGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.MovieSeries.Updated);
    }
}


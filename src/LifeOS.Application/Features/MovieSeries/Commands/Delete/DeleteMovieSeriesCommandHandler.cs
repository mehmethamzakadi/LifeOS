using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.MovieSeries.Commands.Delete;

public sealed class DeleteMovieSeriesCommandHandler(
    IMovieSeriesRepository movieSeriesRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteMovieSeriesCommand, IResult>
{
    public async Task<IResult> Handle(DeleteMovieSeriesCommand request, CancellationToken cancellationToken)
    {
        var movieSeries = await movieSeriesRepository.GetAsync(predicate: x => x.Id == request.Id, enableTracking: true, cancellationToken: cancellationToken);
        if (movieSeries is null)
            return new ErrorResult(ResponseMessages.MovieSeries.NotFound);

        movieSeries.Delete();
        movieSeriesRepository.Update(movieSeries);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.MovieSeries(movieSeries.Id));

        await cacheService.Add(
            CacheKeys.MovieSeriesGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.MovieSeries.Deleted);
    }
}


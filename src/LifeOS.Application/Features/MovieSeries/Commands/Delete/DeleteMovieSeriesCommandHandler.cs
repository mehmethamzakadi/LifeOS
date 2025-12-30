using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.MovieSeries.Commands.Delete;

public sealed class DeleteMovieSeriesCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteMovieSeriesCommand, IResult>
{
    public async Task<IResult> Handle(DeleteMovieSeriesCommand request, CancellationToken cancellationToken)
    {
        var movieSeries = await context.MovieSeries
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (movieSeries is null)
            return new ErrorResult(ResponseMessages.MovieSeries.NotFound);

        movieSeries.Delete();
        context.MovieSeries.Update(movieSeries);
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


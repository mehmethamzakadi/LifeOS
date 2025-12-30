using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.MovieSeries.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.MovieSeries.Commands.Update;

public sealed class UpdateMovieSeriesCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateMovieSeriesCommand, IResult>
{
    public async Task<IResult> Handle(UpdateMovieSeriesCommand request, CancellationToken cancellationToken)
    {
        var movieSeries = await context.MovieSeries
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

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

        context.MovieSeries.Update(movieSeries);
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


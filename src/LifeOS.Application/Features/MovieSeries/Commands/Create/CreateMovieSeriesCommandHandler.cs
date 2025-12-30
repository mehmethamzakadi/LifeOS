using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.MovieSeries.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.MovieSeries.Commands.Create;

public sealed class CreateMovieSeriesCommandHandler(
    LifeOSDbContext context,
    ICacheService cache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateMovieSeriesCommand, IResult>
{
    public async Task<IResult> Handle(CreateMovieSeriesCommand request, CancellationToken cancellationToken)
    {
        var movieSeries = MovieSeriesEntity.Create(
            request.Title,
            request.CoverUrl,
            request.Type,
            request.Platform,
            request.CurrentSeason,
            request.CurrentEpisode,
            request.Status,
            request.Rating,
            request.PersonalNote);

        await context.MovieSeries.AddAsync(movieSeries, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.Add(
            CacheKeys.MovieSeries(movieSeries.Id),
            new GetByIdMovieSeriesResponse(
                Id: movieSeries.Id,
                Title: movieSeries.Title,
                CoverUrl: movieSeries.CoverUrl,
                Type: movieSeries.Type,
                Platform: movieSeries.Platform,
                CurrentSeason: movieSeries.CurrentSeason,
                CurrentEpisode: movieSeries.CurrentEpisode,
                Status: movieSeries.Status,
                Rating: movieSeries.Rating,
                PersonalNote: movieSeries.PersonalNote),
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
            null);

        await cache.Add(
            CacheKeys.MovieSeriesGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.MovieSeries.Created);
    }
}


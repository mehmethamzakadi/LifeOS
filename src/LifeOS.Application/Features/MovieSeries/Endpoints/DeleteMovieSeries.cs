using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.Endpoints;

public static class DeleteMovieSeries
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/movieseries/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var movieSeries = await context.MovieSeries
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (movieSeries is null)
                return ApiResultExtensions.Failure(ResponseMessages.MovieSeries.NotFound).ToResult();

            movieSeries.Delete();
            context.MovieSeries.Update(movieSeries);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.MovieSeries(movieSeries.Id));

            await cacheService.Add(
                CacheKeys.MovieSeriesGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return ApiResultExtensions.Success(ResponseMessages.MovieSeries.Deleted).ToResult();
        })
        .WithName("DeleteMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}


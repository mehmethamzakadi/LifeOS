using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.DeleteMovieSeries;

public static class DeleteMovieSeriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/movieseries/{id}", async (
            Guid id,
            DeleteMovieSeriesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}


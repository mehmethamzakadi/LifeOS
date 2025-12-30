using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;

public static class GetMovieSeriesByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/movieseries/{id}", async (
            Guid id,
            GetMovieSeriesByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetMovieSeriesById")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesRead)
        .Produces<ApiResult<GetMovieSeriesByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetMovieSeriesByIdResponse>>(StatusCodes.Status404NotFound);
    }
}


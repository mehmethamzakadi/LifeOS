using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeriesGenres.DeleteMovieSeriesGenre;

public static class DeleteMovieSeriesGenreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/movie-series-genres/{id:guid}", async (
            Guid id,
            DeleteMovieSeriesGenreHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteMovieSeriesGenre")
        .WithTags("MovieSeriesGenres")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesGenresDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeriesGenres.GetAllMovieSeriesGenres;

public static class GetAllMovieSeriesGenresEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/movie-series-genres", async (
            GetAllMovieSeriesGenresHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetAllMovieSeriesGenres")
        .WithTags("MovieSeriesGenres")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesGenresViewAll)
        .Produces<ApiResult<List<GetAllMovieSeriesGenresResponse>>>(StatusCodes.Status200OK);
    }
}


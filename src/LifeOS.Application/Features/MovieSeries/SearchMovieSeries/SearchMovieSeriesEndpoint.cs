using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.SearchMovieSeries;

public static class SearchMovieSeriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/movieseries/search", async (
            DataGridRequest request,
            SearchMovieSeriesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchMovieSeriesResponse>>>(StatusCodes.Status200OK);
    }
}


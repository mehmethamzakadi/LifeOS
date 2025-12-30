using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.Endpoints;

public static class MovieSeriesEndpoints
{
    public static void MapMovieSeriesEndpoints(this IEndpointRouteBuilder app)
    {
        CreateMovieSeries.MapEndpoint(app);
        UpdateMovieSeries.MapEndpoint(app);
        DeleteMovieSeries.MapEndpoint(app);
        GetMovieSeriesById.MapEndpoint(app);
        SearchMovieSeries.MapEndpoint(app);
    }
}


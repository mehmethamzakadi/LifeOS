using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.GetListeningStats;

public static class GetListeningStatsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/stats", async (
            string? period,
            GetListeningStatsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListeningStatsQuery(period ?? "weekly");
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetListeningStats")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<GetListeningStatsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetListeningStatsResponse>>(StatusCodes.Status401Unauthorized);
    }
}


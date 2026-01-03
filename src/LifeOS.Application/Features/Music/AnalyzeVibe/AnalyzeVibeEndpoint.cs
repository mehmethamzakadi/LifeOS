using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.AnalyzeVibe;

public static class AnalyzeVibeEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/analyze-vibe", async (
            string? timeRange,
            AnalyzeVibeHandler handler,
            CancellationToken cancellationToken) =>
        {
            var query = new AnalyzeVibeQuery(timeRange ?? "short_term");
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToResult();
        })
        .WithName("AnalyzeVibe")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<AnalyzeVibeResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<AnalyzeVibeResponse>>(StatusCodes.Status401Unauthorized);
    }
}


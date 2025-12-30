using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Dashboards.GetStatistics;

public static class GetStatisticsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboards/statistics", async (
            GetStatisticsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetStatistics")
        .WithTags("Dashboards")
        .Produces<ApiResult<GetStatisticsResponse>>(StatusCodes.Status200OK);
    }
}


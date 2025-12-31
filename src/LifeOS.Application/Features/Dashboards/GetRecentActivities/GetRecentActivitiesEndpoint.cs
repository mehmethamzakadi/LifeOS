using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Dashboards.GetRecentActivities;

public static class GetRecentActivitiesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboards/recent-activities", async (
            GetRecentActivitiesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetRecentActivities")
        .WithTags("Dashboards")
        .RequireAuthorization(Domain.Constants.Permissions.DashboardView)
        .Produces<ApiResult<GetRecentActivitiesResponse>>(StatusCodes.Status200OK);
    }
}


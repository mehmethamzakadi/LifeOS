using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Dashboards.Endpoints;

public static class DashboardsEndpoints
{
    public static void MapDashboardsEndpoints(this IEndpointRouteBuilder app)
    {
        GetStatistics.MapEndpoint(app);
    }
}


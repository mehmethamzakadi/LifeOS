using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Dashboards.GetFinancialSummary;

public static class GetFinancialSummaryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboards/financial-summary", async (
            GetFinancialSummaryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetFinancialSummary")
        .WithTags("Dashboards")
        .RequireAuthorization(Domain.Constants.Permissions.DashboardView)
        .Produces<ApiResult<GetFinancialSummaryResponse>>(StatusCodes.Status200OK);
    }
}


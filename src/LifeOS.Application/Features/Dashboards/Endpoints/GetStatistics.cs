using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.Endpoints;

public static class GetStatistics
{
    public sealed record Response(
        int TotalCategories,
        int TotalUsers,
        int TotalRoles);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboards/statistics", async (
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var totalCategories = await context.Categories
                .CountAsync(c => !c.IsDeleted, cancellationToken);
            var totalUsers = await context.Users
                .CountAsync(u => !u.IsDeleted, cancellationToken);
            var totalRoles = await context.Roles
                .CountAsync(r => !r.IsDeleted, cancellationToken);

            var response = new Response(totalCategories, totalUsers, totalRoles);
            return ApiResultExtensions.Success(response, "İstatistikler başarıyla getirildi").ToResult();
        })
        .WithName("GetStatistics")
        .WithTags("Dashboards")
        .Produces<ApiResult<Response>>(StatusCodes.Status200OK);
    }
}


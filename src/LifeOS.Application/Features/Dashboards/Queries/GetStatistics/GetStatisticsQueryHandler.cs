using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.Queries.GetStatistics;

/// <summary>
/// Handler for getting dashboard statistics
/// </summary>
public sealed class GetStatisticsQueryHandler(LifeOSDbContext context)
    : IRequestHandler<GetStatisticsQuery, GetStatisticsResponse>
{
    public async Task<GetStatisticsResponse> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var totalCategories = await context.Categories
            .CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalUsers = await context.Users
            .CountAsync(u => !u.IsDeleted, cancellationToken);
        var totalRoles = await context.Roles
            .CountAsync(r => !r.IsDeleted, cancellationToken);

        return new GetStatisticsResponse
        {
            TotalCategories = totalCategories,
            TotalUsers = totalUsers,
            TotalRoles = totalRoles
        };
    }
}

using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.GetStatistics;

public sealed class GetStatisticsHandler
{
    private readonly LifeOSDbContext _context;

    public GetStatisticsHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetStatisticsResponse>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var totalCategories = await _context.Categories
            .CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalUsers = await _context.Users
            .CountAsync(u => !u.IsDeleted, cancellationToken);
        var totalRoles = await _context.Roles
            .CountAsync(r => !r.IsDeleted, cancellationToken);

        var response = new GetStatisticsResponse(totalCategories, totalUsers, totalRoles);
        return ApiResultExtensions.Success(response, "İstatistikler başarıyla getirildi");
    }
}


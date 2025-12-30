namespace LifeOS.Application.Features.Dashboards.GetStatistics;

public sealed record GetStatisticsResponse(
    int TotalCategories,
    int TotalUsers,
    int TotalRoles);


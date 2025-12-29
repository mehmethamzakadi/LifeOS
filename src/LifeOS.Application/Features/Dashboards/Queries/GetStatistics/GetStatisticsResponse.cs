namespace LifeOS.Application.Features.Dashboards.Queries.GetStatistics;

public sealed record GetStatisticsResponse
{
    public int TotalCategories { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRoles { get; set; }
}

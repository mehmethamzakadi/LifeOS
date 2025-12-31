namespace LifeOS.Application.Features.Dashboards.GetRecentActivities;

public sealed record RecentActivityItem(
    Guid Id,
    string Type,
    string Title,
    string? Subtitle,
    DateTime CreatedDate);

public sealed record GetRecentActivitiesResponse(
    List<RecentActivityItem> Books,
    List<RecentActivityItem> Games,
    List<RecentActivityItem> Movies,
    List<RecentActivityItem> Notes,
    List<RecentActivityItem> WalletTransactions);

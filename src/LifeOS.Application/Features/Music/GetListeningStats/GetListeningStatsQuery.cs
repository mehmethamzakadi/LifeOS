namespace LifeOS.Application.Features.Music.GetListeningStats;

public sealed record GetListeningStatsQuery(
    string Period = "weekly" // daily, weekly, monthly
);


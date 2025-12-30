using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.Queries.GetRecentActivities;

public sealed class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, GetRecentActivitiesResponse>
{
    private readonly LifeOSDbContext _context;

    public GetRecentActivitiesQueryHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<GetRecentActivitiesResponse> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _context.ActivityLogs
            .AsNoTracking()
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Take(request.Count)
            .ToListAsync(cancellationToken);

        var activityDtos = activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            ActivityType = a.ActivityType,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Title = a.Title,
            Timestamp = a.Timestamp,
            UserName = a.User?.UserName
        }).ToList();

        return new GetRecentActivitiesResponse
        {
            Activities = activityDtos
        };
    }
}

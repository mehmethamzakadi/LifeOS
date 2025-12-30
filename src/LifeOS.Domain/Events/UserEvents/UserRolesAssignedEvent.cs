using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.UserEvents;

public class UserRolesAssignedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public IReadOnlyList<string> RoleNames { get; }
    public override Guid AggregateId => UserId;

    public UserRolesAssignedEvent(Guid userId, string userName, IReadOnlyList<string> roleNames)
    {
        UserId = userId;
        UserName = userName;
        RoleNames = roleNames;
    }
}
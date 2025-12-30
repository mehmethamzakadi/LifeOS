using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.RoleEvents;

public class RoleCreatedEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public override Guid AggregateId => RoleId;

    public RoleCreatedEvent(Guid roleId, string roleName)
    {
        RoleId = roleId;
        RoleName = roleName;
    }
}
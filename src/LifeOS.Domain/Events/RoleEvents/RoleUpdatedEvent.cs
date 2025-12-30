using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.RoleEvents;

public class RoleUpdatedEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public override Guid AggregateId => RoleId;

    public RoleUpdatedEvent(Guid roleId, string roleName)
    {
        RoleId = roleId;
        RoleName = roleName;
    }
}
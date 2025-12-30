using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.RoleEvents;

public class RoleDeletedEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public override Guid AggregateId => RoleId;

    public RoleDeletedEvent(Guid roleId, string roleName)
    {
        RoleId = roleId;
        RoleName = roleName;
    }
}
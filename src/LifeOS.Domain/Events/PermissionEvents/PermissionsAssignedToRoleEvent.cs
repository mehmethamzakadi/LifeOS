using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.PermissionEvents;

public class PermissionsAssignedToRoleEvent : DomainEvent
{
    public Guid RoleId { get; }
    public string RoleName { get; }
    public IReadOnlyList<string> PermissionNames { get; }
    public override Guid AggregateId => RoleId;

    public PermissionsAssignedToRoleEvent(Guid roleId, string roleName, IReadOnlyList<string> permissionNames)
    {
        RoleId = roleId;
        RoleName = roleName;
        PermissionNames = permissionNames;
    }
}
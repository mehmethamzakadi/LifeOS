namespace LifeOS.Application.Features.Permissions.AssignPermissionsToRole;

public sealed record AssignPermissionsToRoleCommand(
    Guid RoleId,
    List<Guid> PermissionIds);


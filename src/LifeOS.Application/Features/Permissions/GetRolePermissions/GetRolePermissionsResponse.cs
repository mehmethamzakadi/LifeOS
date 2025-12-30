namespace LifeOS.Application.Features.Permissions.GetRolePermissions;

public sealed record GetRolePermissionsResponse(
    Guid RoleId,
    string RoleName,
    List<Guid> PermissionIds);


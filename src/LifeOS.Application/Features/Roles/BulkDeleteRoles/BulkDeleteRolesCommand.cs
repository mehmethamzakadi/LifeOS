namespace LifeOS.Application.Features.Roles.BulkDeleteRoles;

public sealed record BulkDeleteRolesCommand(List<Guid> RoleIds);


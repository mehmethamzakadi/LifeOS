namespace LifeOS.Application.Features.Permissions.GetAllPermissions;

public sealed record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Type);

public sealed record PermissionModuleDto(
    string ModuleName,
    List<PermissionDto> Permissions);

public sealed record GetAllPermissionsResponse(List<PermissionModuleDto> Modules);


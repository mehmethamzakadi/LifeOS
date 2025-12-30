namespace LifeOS.Application.Features.Users.GetUserRoles;

public sealed record UserRoleDto(Guid Id, string Name);

public sealed record GetUserRolesResponse(
    Guid UserId,
    string UserName,
    string Email,
    List<UserRoleDto> Roles);


namespace LifeOS.Application.Features.Users.AssignRolesToUser;

public sealed record AssignRolesToUserCommand(
    Guid UserId,
    List<Guid> RoleIds);


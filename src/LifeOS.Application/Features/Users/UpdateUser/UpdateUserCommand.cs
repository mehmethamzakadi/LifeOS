namespace LifeOS.Application.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string UserName,
    string Email);


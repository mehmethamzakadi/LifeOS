namespace LifeOS.Application.Features.Users.CreateUser;

public sealed record CreateUserCommand(
    string UserName,
    string Email,
    string Password);


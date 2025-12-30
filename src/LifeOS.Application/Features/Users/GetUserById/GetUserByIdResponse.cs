namespace LifeOS.Application.Features.Users.GetUserById;

public sealed record GetUserByIdResponse(
    Guid Id,
    string UserName,
    string Email,
    DateTimeOffset? LockoutEnd,
    bool LockoutEnabled,
    int AccessFailedCount);


namespace LifeOS.Application.Features.Auths.Login;

public sealed record LoginResponse(
    Guid UserId,
    string UserName,
    DateTime Expiration,
    string Token,
    List<string> Permissions);


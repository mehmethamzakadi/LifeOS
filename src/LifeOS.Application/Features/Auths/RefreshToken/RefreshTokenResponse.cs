namespace LifeOS.Application.Features.Auths.RefreshToken;

public sealed record RefreshTokenResponse(
    Guid UserId,
    string UserName,
    DateTime Expiration,
    string Token,
    List<string> Permissions);


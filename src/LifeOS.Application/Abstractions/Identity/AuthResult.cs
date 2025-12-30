using System.Text.Json.Serialization;

namespace LifeOS.Application.Abstractions.Identity;

public sealed record AuthResult(
    Guid UserId,
    string UserName,
    DateTime Expiration,
    string Token,
    [property: JsonIgnore] string RefreshToken,
    [property: JsonIgnore] DateTime RefreshTokenExpiration,
    List<string> Permissions);


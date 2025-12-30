namespace LifeOS.Application.Features.Auths.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? DeviceId = null);


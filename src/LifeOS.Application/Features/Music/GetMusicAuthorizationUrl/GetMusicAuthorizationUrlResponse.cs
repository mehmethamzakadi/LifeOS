namespace LifeOS.Application.Features.Music.GetMusicAuthorizationUrl;

public sealed record GetMusicAuthorizationUrlResponse(
    string AuthorizationUrl,
    string State
);


namespace LifeOS.Application.Features.Music.GetConnectionStatus;

public sealed record GetConnectionStatusResponse(
    bool IsConnected,
    string? SpotifyUserId,
    string? SpotifyUserName,
    bool? IsTokenExpired
);


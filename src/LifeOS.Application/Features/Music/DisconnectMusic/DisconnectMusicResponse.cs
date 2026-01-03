namespace LifeOS.Application.Features.Music.DisconnectMusic;

public sealed record DisconnectMusicResponse(
    bool IsDisconnected,
    string Message
);


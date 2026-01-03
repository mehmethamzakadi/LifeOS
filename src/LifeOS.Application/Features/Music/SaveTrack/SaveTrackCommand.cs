namespace LifeOS.Application.Features.Music.SaveTrack;

public sealed record SaveTrackCommand(
    string SpotifyTrackId,
    string? Notes = null
);


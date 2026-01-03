namespace LifeOS.Application.Features.Music.SaveTrack;

public sealed record SaveTrackResponse(
    Guid Id,
    Guid UserId,
    string SpotifyTrackId,
    string Name,
    string Artist,
    string? Album,
    string? AlbumCoverUrl,
    int? DurationMs,
    DateTime? SavedAt,
    string? Notes,
    DateTime CreatedDate
);


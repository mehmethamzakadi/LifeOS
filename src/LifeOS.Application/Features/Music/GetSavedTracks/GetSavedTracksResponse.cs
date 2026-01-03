namespace LifeOS.Application.Features.Music.GetSavedTracks;

public sealed record GetSavedTracksResponse(
    List<SavedTrackDto> Tracks
);

public sealed record SavedTrackDto(
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


namespace LifeOS.Application.Features.Music.GetCurrentTrack;

public sealed record GetCurrentTrackResponse(
    bool IsPlaying,
    CurrentTrackItem? Item,
    int? ProgressMs,
    long? Timestamp
);

public sealed record CurrentTrackItem(
    string Id,
    string Name,
    List<CurrentTrackArtist> Artists,
    CurrentTrackAlbum? Album,
    int DurationMs,
    string? PreviewUrl,
    string? ExternalUrl
);

public sealed record CurrentTrackArtist(
    string Id,
    string Name
);

public sealed record CurrentTrackAlbum(
    string Id,
    string Name,
    List<CurrentTrackImage> Images
);

public sealed record CurrentTrackImage(
    string Url,
    int? Height,
    int? Width
);


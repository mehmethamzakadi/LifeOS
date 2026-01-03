namespace LifeOS.Application.Features.Music.GenerateMoodPlaylist;

public sealed record GenerateMoodPlaylistResponse(
    List<PlaylistTrackDto> Tracks,
    string Mood,
    string Description
);

public sealed record PlaylistTrackDto(
    string Id,
    string Name,
    List<PlaylistArtistDto> Artists,
    PlaylistAlbumDto? Album,
    int DurationMs,
    string? PreviewUrl,
    string? ExternalUrl
);

public sealed record PlaylistArtistDto(
    string Id,
    string Name
);

public sealed record PlaylistAlbumDto(
    string Id,
    string Name,
    List<PlaylistImageDto> Images
);

public sealed record PlaylistImageDto(
    string Url,
    int? Height,
    int? Width
);


namespace LifeOS.Application.Features.Music.GetListeningStats;

public sealed record GetListeningStatsResponse(
    List<TopTrackDto> TopTracks,
    List<TopArtistDto> TopArtists,
    long TotalListeningTime,
    string? MostListenedGenre
);

public sealed record TopTrackDto(
    string Id,
    string Name,
    List<TopArtistDto> Artists,
    TopAlbumDto? Album,
    int DurationMs,
    string? PreviewUrl,
    string? ExternalUrl
);

public sealed record TopArtistDto(
    string Id,
    string Name
);

public sealed record TopAlbumDto(
    string Id,
    string Name,
    List<TopImageDto> Images
);

public sealed record TopImageDto(
    string Url,
    int? Height,
    int? Width
);


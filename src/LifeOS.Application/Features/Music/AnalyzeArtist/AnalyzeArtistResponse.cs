namespace LifeOS.Application.Features.Music.AnalyzeArtist;

public sealed record AnalyzeArtistResponse
{
    public string ArtistName { get; init; } = default!;
    public string ArtistId { get; init; } = default!;
    public string? ArtistImageUrl { get; init; }
    public List<TrackAnalysis> Tracks { get; init; } = new();
}

public sealed record TrackAnalysis
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string ArtistName { get; init; } = default!;
    public double? Valence { get; init; } // 0.0-1.0 (mutluluk/hüzün skoru) - nullable, Audio Features erişilemezse null
    public double? Energy { get; init; } // 0.0-1.0 - nullable
    public double? Danceability { get; init; } // 0.0-1.0 - nullable
    public double? Tempo { get; init; } // BPM - nullable
    public string? ValenceDescription { get; init; } // "Mutlu" veya "Hüzünlü" - nullable
    public bool HasAudioFeatures { get; init; } // Audio Features verisi mevcut mu?
}


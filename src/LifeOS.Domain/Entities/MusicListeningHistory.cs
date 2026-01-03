using LifeOS.Domain.Common;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Müzik dinleme geçmişi - istatistikler için
/// </summary>
public sealed class MusicListeningHistory : BaseEntity
{
    // EF Core için parameterless constructor
    public MusicListeningHistory() { }

    public Guid UserId { get; private set; }
    public string SpotifyTrackId { get; private set; } = default!;
    public string TrackName { get; private set; } = default!;
    public string ArtistName { get; private set; } = default!;
    public string? AlbumName { get; private set; }
    public string? Genre { get; private set; }
    public DateTime PlayedAt { get; private set; }
    public int DurationMs { get; private set; }
    public int? ProgressMs { get; private set; } // Ne kadar dinlendiği

    public static MusicListeningHistory Create(
        Guid userId,
        string spotifyTrackId,
        string trackName,
        string artistName,
        string? albumName = null,
        string? genre = null,
        DateTime? playedAt = null,
        int durationMs = 0,
        int? progressMs = null)
    {
        return new MusicListeningHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SpotifyTrackId = spotifyTrackId,
            TrackName = trackName,
            ArtistName = artistName,
            AlbumName = albumName,
            Genre = genre,
            PlayedAt = playedAt ?? DateTime.UtcNow,
            DurationMs = durationMs,
            ProgressMs = progressMs,
            CreatedDate = DateTime.UtcNow
        };
    }
}


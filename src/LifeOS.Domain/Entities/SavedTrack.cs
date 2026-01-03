using LifeOS.Domain.Common;
using LifeOS.Domain.Events.MusicEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Kullanıcının beğenip kaydettiği şarkılar
/// </summary>
public sealed class SavedTrack : AggregateRoot
{
    // EF Core için parameterless constructor
    public SavedTrack() { }

    public Guid UserId { get; private set; }
    public string SpotifyTrackId { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Artist { get; private set; } = default!;
    public string? Album { get; private set; }
    public string? AlbumCoverUrl { get; private set; }
    public int? DurationMs { get; private set; }
    public DateTime? SavedAt { get; private set; }
    public string? Notes { get; private set; } // Kullanıcının eklediği notlar

    public static SavedTrack Create(
        Guid userId,
        string spotifyTrackId,
        string name,
        string artist,
        string? album = null,
        string? albumCoverUrl = null,
        int? durationMs = null,
        string? notes = null)
    {
        var track = new SavedTrack
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SpotifyTrackId = spotifyTrackId,
            Name = name,
            Artist = artist,
            Album = album,
            AlbumCoverUrl = albumCoverUrl,
            DurationMs = durationMs,
            SavedAt = DateTime.UtcNow,
            Notes = notes,
            CreatedDate = DateTime.UtcNow
        };

        track.AddDomainEvent(new SavedTrackCreatedEvent(track.Id, userId, name, artist));
        return track;
    }

    public void Update(string name, string artist, string? album = null, string? albumCoverUrl = null, int? durationMs = null, string? notes = null)
    {
        Name = name;
        Artist = artist;
        Album = album;
        AlbumCoverUrl = albumCoverUrl;
        DurationMs = durationMs;
        Notes = notes;
        UpdatedDate = DateTime.UtcNow;

        AddDomainEvent(new SavedTrackUpdatedEvent(Id, name, artist));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new SavedTrackDeletedEvent(Id, Name, Artist));
    }
}


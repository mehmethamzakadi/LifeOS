using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.MusicEvents;

public sealed class SavedTrackCreatedEvent : DomainEvent
{
    public Guid TrackId { get; }
    public Guid UserId { get; }
    public string TrackName { get; }
    public string Artist { get; }
    public override Guid AggregateId => TrackId;

    public SavedTrackCreatedEvent(Guid trackId, Guid userId, string trackName, string artist)
    {
        TrackId = trackId;
        UserId = userId;
        TrackName = trackName;
        Artist = artist;
    }
}


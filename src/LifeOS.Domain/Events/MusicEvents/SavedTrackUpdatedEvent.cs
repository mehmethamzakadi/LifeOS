using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.MusicEvents;

public sealed class SavedTrackUpdatedEvent : DomainEvent
{
    public Guid TrackId { get; }
    public string TrackName { get; }
    public string Artist { get; }
    public override Guid AggregateId => TrackId;

    public SavedTrackUpdatedEvent(Guid trackId, string trackName, string artist)
    {
        TrackId = trackId;
        TrackName = trackName;
        Artist = artist;
    }
}


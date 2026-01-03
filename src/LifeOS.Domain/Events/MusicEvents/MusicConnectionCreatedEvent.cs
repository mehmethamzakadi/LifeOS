using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.MusicEvents;

public sealed class MusicConnectionCreatedEvent : DomainEvent
{
    public Guid ConnectionId { get; }
    public Guid UserId { get; }
    public string SpotifyUserId { get; }
    public override Guid AggregateId => ConnectionId;

    public MusicConnectionCreatedEvent(Guid connectionId, Guid userId, string spotifyUserId)
    {
        ConnectionId = connectionId;
        UserId = userId;
        SpotifyUserId = spotifyUserId;
    }
}


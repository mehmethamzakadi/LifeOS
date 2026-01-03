using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.MusicEvents;

public sealed class MusicConnectionDeactivatedEvent : DomainEvent
{
    public Guid ConnectionId { get; }
    public Guid UserId { get; }
    public override Guid AggregateId => ConnectionId;

    public MusicConnectionDeactivatedEvent(Guid connectionId, Guid userId)
    {
        ConnectionId = connectionId;
        UserId = userId;
    }
}


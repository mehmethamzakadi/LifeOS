using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.MusicEvents;

public sealed class MusicConnectionDeletedEvent : DomainEvent
{
    public Guid ConnectionId { get; }
    public Guid UserId { get; }
    public override Guid AggregateId => ConnectionId;

    public MusicConnectionDeletedEvent(Guid connectionId, Guid userId)
    {
        ConnectionId = connectionId;
        UserId = userId;
    }
}


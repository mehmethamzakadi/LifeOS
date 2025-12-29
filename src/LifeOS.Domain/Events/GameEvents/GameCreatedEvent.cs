using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.GameEvents;

[StoreInOutbox]
public class GameCreatedEvent : DomainEvent
{
    public Guid GameId { get; }
    public string Title { get; }
    public override Guid AggregateId => GameId;

    public GameCreatedEvent(Guid gameId, string title)
    {
        GameId = gameId;
        Title = title;
    }
}


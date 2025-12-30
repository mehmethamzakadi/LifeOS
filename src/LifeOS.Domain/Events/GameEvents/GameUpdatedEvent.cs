using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.GameEvents;

public class GameUpdatedEvent : DomainEvent
{
    public Guid GameId { get; }
    public string Title { get; }
    public override Guid AggregateId => GameId;

    public GameUpdatedEvent(Guid gameId, string title)
    {
        GameId = gameId;
        Title = title;
    }
}


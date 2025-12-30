using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.GameEvents;

public class GameDeletedEvent : DomainEvent
{
    public Guid GameId { get; }
    public string Title { get; }
    public override Guid AggregateId => GameId;

    public GameDeletedEvent(Guid gameId, string title)
    {
        GameId = gameId;
        Title = title;
    }
}


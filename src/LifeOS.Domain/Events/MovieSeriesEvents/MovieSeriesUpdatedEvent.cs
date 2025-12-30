using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.MovieSeriesEvents;

public class MovieSeriesUpdatedEvent : DomainEvent
{
    public Guid MovieSeriesId { get; }
    public string Title { get; }
    public override Guid AggregateId => MovieSeriesId;

    public MovieSeriesUpdatedEvent(Guid movieSeriesId, string title)
    {
        MovieSeriesId = movieSeriesId;
        Title = title;
    }
}


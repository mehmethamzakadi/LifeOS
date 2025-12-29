using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.MovieSeriesEvents;

[StoreInOutbox]
public class MovieSeriesDeletedEvent : DomainEvent
{
    public Guid MovieSeriesId { get; }
    public string Title { get; }
    public override Guid AggregateId => MovieSeriesId;

    public MovieSeriesDeletedEvent(Guid movieSeriesId, string title)
    {
        MovieSeriesId = movieSeriesId;
        Title = title;
    }
}


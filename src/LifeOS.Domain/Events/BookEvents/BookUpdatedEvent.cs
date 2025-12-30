using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.BookEvents;

public class BookUpdatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    public string Author { get; }
    public override Guid AggregateId => BookId;

    public BookUpdatedEvent(Guid bookId, string title, string author)
    {
        BookId = bookId;
        Title = title;
        Author = author;
    }
}


using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.BookEvents;

public class BookCreatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    public string Author { get; }
    public override Guid AggregateId => BookId;

    public BookCreatedEvent(Guid bookId, string title, string author)
    {
        BookId = bookId;
        Title = title;
        Author = author;
    }
}


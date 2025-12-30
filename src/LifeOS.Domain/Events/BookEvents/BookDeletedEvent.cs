using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.BookEvents;

public class BookDeletedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    public string Author { get; }
    public override Guid AggregateId => BookId;

    public BookDeletedEvent(Guid bookId, string title, string author)
    {
        BookId = bookId;
        Title = title;
        Author = author;
    }
}


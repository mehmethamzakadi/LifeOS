using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.BookEvents;

[StoreInOutbox]
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


using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.PersonalNoteEvents;

[StoreInOutbox]
public class PersonalNoteCreatedEvent : DomainEvent
{
    public Guid PersonalNoteId { get; }
    public string Title { get; }
    public override Guid AggregateId => PersonalNoteId;

    public PersonalNoteCreatedEvent(Guid personalNoteId, string title)
    {
        PersonalNoteId = personalNoteId;
        Title = title;
    }
}


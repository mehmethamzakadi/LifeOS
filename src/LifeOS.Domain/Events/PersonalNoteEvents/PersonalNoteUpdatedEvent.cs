using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.PersonalNoteEvents;

[StoreInOutbox]
public class PersonalNoteUpdatedEvent : DomainEvent
{
    public Guid PersonalNoteId { get; }
    public string Title { get; }
    public override Guid AggregateId => PersonalNoteId;

    public PersonalNoteUpdatedEvent(Guid personalNoteId, string title)
    {
        PersonalNoteId = personalNoteId;
        Title = title;
    }
}


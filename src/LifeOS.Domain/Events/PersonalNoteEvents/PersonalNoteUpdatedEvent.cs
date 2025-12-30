using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.PersonalNoteEvents;

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


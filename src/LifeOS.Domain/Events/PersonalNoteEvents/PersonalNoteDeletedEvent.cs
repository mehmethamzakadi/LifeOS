using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.PersonalNoteEvents;

public class PersonalNoteDeletedEvent : DomainEvent
{
    public Guid PersonalNoteId { get; }
    public string Title { get; }
    public override Guid AggregateId => PersonalNoteId;

    public PersonalNoteDeletedEvent(Guid personalNoteId, string title)
    {
        PersonalNoteId = personalNoteId;
        Title = title;
    }
}


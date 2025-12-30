using LifeOS.Domain.Common;
using LifeOS.Domain.Events.PersonalNoteEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Kişisel not entity'si
/// </summary>
public sealed class PersonalNote : AggregateRoot
{
    // EF Core için parameterless constructor
    public PersonalNote() { }

    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!; // HTML/RichText
    public string? Category { get; private set; }
    public bool IsPinned { get; private set; }
    public string? Tags { get; private set; } // Comma separated string

    public static PersonalNote Create(string title, string content, string? category, bool isPinned, string? tags)
    {
        var personalNote = new PersonalNote
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            Category = category,
            IsPinned = isPinned,
            Tags = tags,
            CreatedDate = DateTime.UtcNow
        };

        personalNote.AddDomainEvent(new PersonalNoteCreatedEvent(personalNote.Id, title));
        return personalNote;
    }

    public void Update(string title, string content, string? category, bool isPinned, string? tags)
    {
        Title = title;
        Content = content;
        Category = category;
        IsPinned = isPinned;
        Tags = tags;
        UpdatedDate = DateTime.UtcNow;

        AddDomainEvent(new PersonalNoteUpdatedEvent(Id, title));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new PersonalNoteDeletedEvent(Id, Title));
    }
}


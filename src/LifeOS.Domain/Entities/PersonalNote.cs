using LifeOS.Domain.Common;
using LifeOS.Domain.Events.PersonalNoteEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Kişisel not entity'si
/// </summary>
public sealed class PersonalNote : BaseEntity
{
    // EF Core için parameterless constructor
    public PersonalNote() { }

    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!; // HTML/RichText
    public string? Category { get; set; }
    public bool IsPinned { get; set; }
    public string? Tags { get; set; } // Comma separated string

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


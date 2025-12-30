namespace LifeOS.Application.Features.PersonalNotes.UpdatePersonalNote;

public sealed record UpdatePersonalNoteCommand(
    Guid Id,
    string Title,
    string Content,
    string? Category = null,
    bool IsPinned = false,
    string? Tags = null);


namespace LifeOS.Application.Features.PersonalNotes.CreatePersonalNote;

public sealed record CreatePersonalNoteCommand(
    string Title,
    string Content,
    string? Category = null,
    bool IsPinned = false,
    string? Tags = null);


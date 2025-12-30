namespace LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;

public sealed record GetPersonalNoteByIdResponse(
    Guid Id,
    string Title,
    string Content,
    string? Category,
    bool IsPinned,
    string? Tags);


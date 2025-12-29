namespace LifeOS.Application.Features.PersonalNotes.Queries.GetById;

public sealed record GetByIdPersonalNoteResponse(
    Guid Id,
    string Title,
    string Content,
    string? Category,
    bool IsPinned,
    string? Tags);


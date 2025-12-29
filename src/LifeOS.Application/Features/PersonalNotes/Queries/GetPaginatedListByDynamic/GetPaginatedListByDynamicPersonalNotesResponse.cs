using LifeOS.Application.Common;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicPersonalNotesResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Category { get; init; }
    public bool IsPinned { get; init; }
    public string? Tags { get; init; }
}


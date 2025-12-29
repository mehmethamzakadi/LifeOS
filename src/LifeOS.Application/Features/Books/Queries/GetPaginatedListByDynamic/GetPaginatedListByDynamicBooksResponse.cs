using LifeOS.Application.Common;
using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Books.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicBooksResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public int TotalPages { get; init; }
    public int CurrentPage { get; init; }
    public BookStatus Status { get; init; }
    public int? Rating { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}


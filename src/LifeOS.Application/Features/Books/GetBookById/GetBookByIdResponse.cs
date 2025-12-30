using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Books.GetBookById;

public sealed record GetBookByIdResponse(
    Guid Id,
    string Title,
    string Author,
    string? CoverUrl,
    int TotalPages,
    int CurrentPage,
    BookStatus Status,
    int? Rating,
    DateTime? StartDate,
    DateTime? EndDate);


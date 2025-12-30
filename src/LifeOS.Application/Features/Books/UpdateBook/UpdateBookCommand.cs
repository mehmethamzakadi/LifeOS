using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Books.UpdateBook;

public sealed record UpdateBookCommand(
    Guid Id,
    string Title,
    string Author,
    string? CoverUrl = null,
    int TotalPages = 0,
    int CurrentPage = 0,
    BookStatus Status = BookStatus.ToRead,
    int? Rating = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null);


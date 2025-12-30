using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Books.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string Author,
    string? CoverUrl = null,
    int TotalPages = 0,
    int CurrentPage = 0,
    BookStatus Status = BookStatus.ToRead,
    int? Rating = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null);


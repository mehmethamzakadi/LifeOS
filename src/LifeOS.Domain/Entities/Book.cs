using LifeOS.Domain.Common;
using LifeOS.Domain.Enums;
using LifeOS.Domain.Events.BookEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Kitap entity'si
/// </summary>
public sealed class Book : BaseEntity
{
    // EF Core için parameterless constructor
    public Book() { }

    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string? CoverUrl { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public BookStatus Status { get; set; }
    public int? Rating { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public static Book Create(string title, string author, string? coverUrl, int totalPages, int currentPage, BookStatus status, int? rating, DateTime? startDate, DateTime? endDate)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            CoverUrl = coverUrl,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            Status = status,
            Rating = rating,
            StartDate = startDate,
            EndDate = endDate,
            CreatedDate = DateTime.UtcNow
        };

        book.AddDomainEvent(new BookCreatedEvent(book.Id, title, author));
        return book;
    }

    public void Update(string title, string author, string? coverUrl, int totalPages, int currentPage, BookStatus status, int? rating, DateTime? startDate, DateTime? endDate)
    {
        Title = title;
        Author = author;
        CoverUrl = coverUrl;
        TotalPages = totalPages;
        CurrentPage = currentPage;
        Status = status;
        Rating = rating;
        StartDate = startDate;
        EndDate = endDate;
        UpdatedDate = DateTime.UtcNow;

        AddDomainEvent(new BookUpdatedEvent(Id, title, author));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new BookDeletedEvent(Id, Title, Author));
    }
}


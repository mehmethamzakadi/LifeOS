using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.CreateBook;

public sealed class CreateBookHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateBookHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateBookResponse> HandleAsync(
        CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var book = Book.Create(
            command.Title,
            command.Author,
            command.CoverUrl,
            command.TotalPages,
            command.CurrentPage,
            command.Status,
            command.Rating,
            command.StartDate,
            command.EndDate);

        await _context.Books.AddAsync(book, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.BookGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateBookResponse(book.Id);
    }
}


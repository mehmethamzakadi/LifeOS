using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.UpdateBook;

public sealed class UpdateBookHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateBookHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateBookCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var book = await _context.Books
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (book is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.Book.NotFound);
        }

        book.Update(
            command.Title,
            command.Author,
            command.CoverUrl,
            command.TotalPages,
            command.CurrentPage,
            command.Status,
            command.Rating,
            command.StartDate,
            command.EndDate);

        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Remove(CacheKeys.Book(book.Id));
        await _cache.Add(
            CacheKeys.BookGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.Book.Updated);
    }
}


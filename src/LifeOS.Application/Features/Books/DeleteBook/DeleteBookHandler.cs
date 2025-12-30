using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.DeleteBook;

public sealed class DeleteBookHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public DeleteBookHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (book is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.Book.NotFound);
        }

        book.Delete();
        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Remove(CacheKeys.Book(book.Id));
        await _cache.Add(
            CacheKeys.BookGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.Book.Deleted);
    }
}


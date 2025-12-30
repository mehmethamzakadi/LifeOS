using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.GetBookById;

public sealed class GetBookByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public GetBookByIdHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<GetBookByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Book(id);
        var cacheValue = await _cacheService.Get<GetBookByIdResponse>(cacheKey);
        if (cacheValue is not null)
            return ApiResultExtensions.Success(cacheValue, "Kitap bilgisi başarıyla getirildi");

        var book = await _context.Books
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (book is null)
            return ApiResultExtensions.Failure<GetBookByIdResponse>("Kitap bilgisi bulunamadı.");

        var response = new GetBookByIdResponse(
            book.Id,
            book.Title,
            book.Author,
            book.CoverUrl,
            book.TotalPages,
            book.CurrentPage,
            book.Status,
            book.Rating,
            book.StartDate,
            book.EndDate);

        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.Book),
            null);

        return ApiResultExtensions.Success(response, "Kitap bilgisi başarıyla getirildi");
    }
}


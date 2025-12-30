using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.SearchBooks;

public sealed class SearchBooksHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchBooksHandler(
        LifeOSDbContext context,
        IMapper mapper,
        ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchBooksResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var versionKey = CacheKeys.BookGridVersion();
        var versionToken = await _cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await _cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.BookGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
        var cachedResponse = await _cacheService.Get<PaginatedListResponse<SearchBooksResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return ApiResultExtensions.Success(cachedResponse, "Kitaplar başarıyla getirildi");
        }

        var query = _context.Books.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var booksDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<SearchBooksResponse> response = _mapper.Map<PaginatedListResponse<SearchBooksResponse>>(booksDynamic);
        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.BookGrid),
            null);

        return ApiResultExtensions.Success(response, "Kitaplar başarıyla getirildi");
    }
}


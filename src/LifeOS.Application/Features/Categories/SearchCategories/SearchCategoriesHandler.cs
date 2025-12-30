using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.SearchCategories;

public sealed class SearchCategoriesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchCategoriesHandler(
        LifeOSDbContext context,
        IMapper mapper,
        ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchCategoriesResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var versionKey = CacheKeys.CategoryGridVersion();
        var versionToken = await _cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await _cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.CategoryGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
        var cachedResponse = await _cacheService.Get<PaginatedListResponse<SearchCategoriesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return ApiResultExtensions.Success(cachedResponse, "Kategoriler başarıyla getirildi");
        }

        var query = _context.Categories
            .Include(c => c.Parent)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var categoriesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<SearchCategoriesResponse> response = _mapper.Map<PaginatedListResponse<SearchCategoriesResponse>>(categoriesDynamic);
        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.CategoryGrid),
            null);

        return ApiResultExtensions.Success(response, "Kategoriler başarıyla getirildi");
    }
}


using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicCategoriesQueryHandler(
    LifeOSDbContext context,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicCategoriesQuery, PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>> Handle(GetPaginatedListByDynamicCategoriesQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.CategoryGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.CategoryGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var query = context.Categories
            .Include(c => c.Parent)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(request.DataGridRequest.DynamicQuery);
        var categoriesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>(categoriesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.CategoryGrid),
            null);

        return response;
    }
}

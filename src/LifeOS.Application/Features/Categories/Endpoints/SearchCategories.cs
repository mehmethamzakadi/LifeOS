using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class SearchCategories
{
    public sealed record Response : BaseEntityResponse
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public Guid? ParentId { get; init; }
        public string? ParentName { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var versionKey = CacheKeys.CategoryGridVersion();
            var versionToken = await cacheService.Get<string>(versionKey);
            if (string.IsNullOrWhiteSpace(versionToken))
            {
                versionToken = Guid.NewGuid().ToString("N");
                await cacheService.Add(versionKey, versionToken, null, null);
            }

            var cacheKey = CacheKeys.CategoryGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
            var cachedResponse = await cacheService.Get<PaginatedListResponse<Response>>(cacheKey);
            if (cachedResponse is not null)
            {
                return ApiResultExtensions.Success(cachedResponse, "Kategoriler başarıyla getirildi").ToResult();
            }

            var query = context.Categories
                .Include(c => c.Parent)
                .AsNoTracking()
                .AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var categoriesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(categoriesDynamic);
            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.CategoryGrid),
                null);

            return ApiResultExtensions.Success(response, "Kategoriler başarıyla getirildi").ToResult();
        })
        .WithName("SearchCategories")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesViewAll)
        .Produces<ApiResult<PaginatedListResponse<Response>>>(StatusCodes.Status200OK);
    }
}


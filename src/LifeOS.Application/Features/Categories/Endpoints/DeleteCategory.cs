using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class DeleteCategory
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/categories/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var category = await context.Categories
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (category is null)
                return ApiResultExtensions.Failure(ResponseMessages.Category.NotFound).ToResult();

            // Alt kategori kontrolü
            var hasChildren = await context.Categories
                .AnyAsync(x => x.ParentId == id && !x.IsDeleted, cancellationToken);
            if (hasChildren)
                return ApiResultExtensions.Failure("Bu kategorinin alt kategorileri bulunmaktadır. Önce alt kategorileri silmeniz gerekmektedir.").ToResult();

            category.Delete();
            context.Categories.Update(category);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.Category(category.Id));

            await cacheService.Add(
                CacheKeys.CategoryGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return ApiResultExtensions.Success(ResponseMessages.Category.Deleted).ToResult();
        })
        .WithName("DeleteCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


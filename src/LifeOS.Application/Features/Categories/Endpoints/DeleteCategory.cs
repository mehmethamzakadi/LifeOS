using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
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
                return Results.NotFound(new { Error = ResponseMessages.Category.NotFound });

            // Alt kategori kontrolü
            var hasChildren = await context.Categories
                .AnyAsync(x => x.ParentId == id && !x.IsDeleted, cancellationToken);
            if (hasChildren)
                return Results.BadRequest(new { Error = "Bu kategorinin alt kategorileri bulunmaktadır. Önce alt kategorileri silmeniz gerekmektedir." });

            category.Delete();
            context.Categories.Update(category);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.Category(category.Id));

            await cacheService.Add(
                CacheKeys.CategoryGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.NoContent();
        })
        .WithName("DeleteCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);
    }
}


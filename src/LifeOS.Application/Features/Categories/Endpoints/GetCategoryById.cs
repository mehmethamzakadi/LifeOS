using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class GetCategoryById
{
    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        Guid? ParentId);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.Category(id);
            var cacheValue = await cacheService.Get<Response>(cacheKey);
            if (cacheValue is not null)
                return Results.Ok(cacheValue);

            var category = await context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (category is null)
                return Results.NotFound(new { Error = "Kategori bilgisi bulunamadı." });

            var response = new Response(
                category.Id,
                category.Name,
                category.Description,
                category.ParentId);

            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.Category),
                null);

            return Results.Ok(response);
        })
        .WithName("GetCategoryById")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


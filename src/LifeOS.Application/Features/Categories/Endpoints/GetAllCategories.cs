using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class GetAllCategories
{
    public sealed record Response(
        Guid Id,
        string Name,
        string? Description,
        Guid? ParentId);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var categories = await context.Categories
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var response = categories.Select(c => new Response(c.Id, c.Name, c.Description, c.ParentId)).ToList();

            return ApiResultExtensions.Success(response, "Kategoriler başarıyla getirildi").ToResult();
        })
        .WithName("GetAllCategories")
        .WithTags("Categories")
        .AllowAnonymous()
        .Produces<ApiResult<List<Response>>>(StatusCodes.Status200OK);
    }
}


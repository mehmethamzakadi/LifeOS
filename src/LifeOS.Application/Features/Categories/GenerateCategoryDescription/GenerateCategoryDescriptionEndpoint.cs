using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.GenerateCategoryDescription;

public static class GenerateCategoryDescriptionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/generate-description", async (
            string categoryName,
            GenerateCategoryDescriptionHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(categoryName, cancellationToken);
            return result.ToResult();
        })
        .WithName("GenerateCategoryDescription")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesCreate)
        .Produces<ApiResult<GenerateCategoryDescriptionResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GenerateCategoryDescriptionResponse>>(StatusCodes.Status400BadRequest);
    }
}


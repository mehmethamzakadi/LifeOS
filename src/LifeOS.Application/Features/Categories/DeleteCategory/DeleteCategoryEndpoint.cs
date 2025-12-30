using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.DeleteCategory;

public static class DeleteCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/categories/{id}", async (
            Guid id,
            DeleteCategoryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


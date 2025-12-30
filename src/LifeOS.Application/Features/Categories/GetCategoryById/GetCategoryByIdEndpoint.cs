using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.GetCategoryById;

public static class GetCategoryByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories/{id}", async (
            Guid id,
            GetCategoryByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetCategoryById")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesRead)
        .Produces<ApiResult<GetCategoryByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetCategoryByIdResponse>>(StatusCodes.Status404NotFound);
    }
}


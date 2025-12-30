using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.GetAllCategories;

public static class GetAllCategoriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (
            GetAllCategoriesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetAllCategories")
        .WithTags("Categories")
        .AllowAnonymous()
        .Produces<ApiResult<List<GetAllCategoriesResponse>>>(StatusCodes.Status200OK);
    }
}


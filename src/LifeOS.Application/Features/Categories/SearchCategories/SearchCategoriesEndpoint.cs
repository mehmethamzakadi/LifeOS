using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.SearchCategories;

public static class SearchCategoriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories/search", async (
            DataGridRequest request,
            SearchCategoriesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchCategories")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchCategoriesResponse>>>(StatusCodes.Status200OK);
    }
}


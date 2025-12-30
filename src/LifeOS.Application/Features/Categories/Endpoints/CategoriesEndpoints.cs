using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class CategoriesEndpoints
{
    public static void MapCategoriesEndpoints(this IEndpointRouteBuilder app)
    {
        CreateCategory.MapEndpoint(app);
        UpdateCategory.MapEndpoint(app);
        DeleteCategory.MapEndpoint(app);
        GetCategoryById.MapEndpoint(app);
        SearchCategories.MapEndpoint(app);
        GetAllCategories.MapEndpoint(app);
    }
}


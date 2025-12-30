using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.SearchUsers;

public static class SearchUsersEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/search", async (
            DataGridRequest request,
            SearchUsersHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchUsers")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.UsersViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchUsersResponse>>>(StatusCodes.Status200OK);
    }
}


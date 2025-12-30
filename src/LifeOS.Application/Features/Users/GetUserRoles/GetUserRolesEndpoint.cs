using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.GetUserRoles;

public static class GetUserRolesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id}/roles", async (
            Guid id,
            GetUserRolesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetUserRoles")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.RolesRead)
        .Produces<ApiResult<GetUserRolesResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetUserRolesResponse>>(StatusCodes.Status404NotFound);
    }
}


using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Permissions.GetRolePermissions;

public static class GetRolePermissionsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/permissions/role/{roleId}", async (
            Guid roleId,
            GetRolePermissionsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(roleId, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetRolePermissions")
        .WithTags("Permissions")
        .RequireAuthorization(Domain.Constants.Permissions.RolesRead)
        .Produces<ApiResult<GetRolePermissionsResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetRolePermissionsResponse>>(StatusCodes.Status404NotFound);
    }
}


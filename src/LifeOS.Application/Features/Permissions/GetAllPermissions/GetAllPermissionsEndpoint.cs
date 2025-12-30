using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Permissions.GetAllPermissions;

public static class GetAllPermissionsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/permissions", async (
            GetAllPermissionsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetAllPermissions")
        .WithTags("Permissions")
        .RequireAuthorization(Domain.Constants.Permissions.RolesRead)
        .Produces<ApiResult<GetAllPermissionsResponse>>(StatusCodes.Status200OK);
    }
}


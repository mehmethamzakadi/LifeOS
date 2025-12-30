using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Permissions.Endpoints;

public static class PermissionsEndpoints
{
    public static void MapPermissionsEndpoints(this IEndpointRouteBuilder app)
    {
        GetAllPermissions.MapEndpoint(app);
        GetRolePermissions.MapEndpoint(app);
        AssignPermissionsToRole.MapEndpoint(app);
    }
}


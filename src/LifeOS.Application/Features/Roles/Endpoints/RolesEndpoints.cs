using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class RolesEndpoints
{
    public static void MapRolesEndpoints(this IEndpointRouteBuilder app)
    {
        CreateRole.MapEndpoint(app);
        GetRoleById.MapEndpoint(app);
        UpdateRole.MapEndpoint(app);
        DeleteRole.MapEndpoint(app);
        GetListRoles.MapEndpoint(app);
        BulkDeleteRoles.MapEndpoint(app);
    }
}


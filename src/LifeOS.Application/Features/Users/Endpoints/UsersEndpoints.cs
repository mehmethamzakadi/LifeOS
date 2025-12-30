using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        CreateUser.MapEndpoint(app);
        GetUserById.MapEndpoint(app);
        UpdateUser.MapEndpoint(app);
        DeleteUser.MapEndpoint(app);
        SearchUsers.MapEndpoint(app);
        AssignRolesToUser.MapEndpoint(app);
        GetUserRoles.MapEndpoint(app);
        BulkDeleteUsers.MapEndpoint(app);
        ExportUsers.MapEndpoint(app);
        ProfileEndpoints.MapProfileEndpoints(app);
    }
}


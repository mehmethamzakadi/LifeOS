using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.ExportUsers;

public static class ExportUsersEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/export", async (
            [Microsoft.AspNetCore.Mvc.FromQuery] string format,
            ExportUsersHandler handler,
            CancellationToken cancellationToken) =>
        {
            var (bytes, contentType, fileName) = await handler.HandleAsync(format ?? "csv", cancellationToken);
            return Results.File(bytes, contentType, fileName);
        })
        .WithName("ExportUsers")
        .WithTags("Users")
        .RequireAuthorization(Domain.Constants.Permissions.UsersViewAll)
        .Produces(StatusCodes.Status200OK);
    }
}


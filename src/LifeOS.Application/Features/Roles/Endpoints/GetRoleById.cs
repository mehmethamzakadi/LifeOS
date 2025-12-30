using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class GetRoleById
{
    public sealed record Response(Guid Id, string Name);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/roles/{id}", async (
            Guid id,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var role = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

            if (role is null)
                return Results.NotFound(new { Error = "Rol bulunamadı!" });

            return Results.Ok(new Response(role.Id, role.Name ?? string.Empty));
        })
        .WithName("GetRoleById")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


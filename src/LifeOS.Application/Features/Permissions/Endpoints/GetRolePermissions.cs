using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.Endpoints;

public static class GetRolePermissions
{
    public sealed record Response(
        Guid RoleId,
        string RoleName,
        List<Guid> PermissionIds);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/permissions/role/{roleId}", async (
            Guid roleId,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var role = await context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

            if (role == null)
                return Results.NotFound(new { Error = "Rol bulunamadı" });

            var permissionIds = await context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            var response = new Response(
                role.Id,
                role.Name ?? string.Empty,
                permissionIds);

            return Results.Ok(response);
        })
        .WithName("GetRolePermissions")
        .WithTags("Permissions")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


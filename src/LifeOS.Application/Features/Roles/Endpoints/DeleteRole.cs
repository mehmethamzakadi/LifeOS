using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class DeleteRole
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/roles/{id}", async (
            Guid id,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var role = await context.Roles
                .Include(r => r.UserRoles)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

            if (role == null)
                return Results.NotFound(new { Error = "Rol bulunamadı!" });

            if (role.NormalizedName == "ADMIN")
                return Results.BadRequest(new { Error = "Admin rolü silinemez!" });

            if (role.UserRoles.Any(ur => !ur.IsDeleted))
                return Results.BadRequest(new { Error = "Bu role atanmış aktif kullanıcılar bulunmaktadır. Önce kullanıcılardan bu rolü kaldırmalısınız." });

            role.Delete();
            context.Roles.Update(role);
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteRole")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}


using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class DeleteUser
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/users/{id}", async (
            Guid id,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

            if (user == null)
                return Results.NotFound(new { Error = "Kullanıcı bilgisi bulunamadı!" });

            user.Delete();
            context.Users.Update(user);
            await context.SaveChangesAsync(cancellationToken);

            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}


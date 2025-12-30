using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class GetUserRoles
{
    public sealed record UserRoleDto(Guid Id, string Name);

    public sealed record Response(
        Guid UserId,
        string UserName,
        string Email,
        List<UserRoleDto> Roles);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id}/roles", async (
            Guid id,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

            if (user == null)
                return Results.NotFound(new { Error = "Kullanıcı bulunamadı" });

            var userRoles = user.UserRoles
                .Where(ur => !ur.IsDeleted)
                .Select(ur => new UserRoleDto(ur.Role.Id, ur.Role.Name ?? string.Empty))
                .ToList();

            var response = new Response(
                user.Id,
                user.UserName,
                user.Email,
                userRoles);

            return Results.Ok(response);
        })
        .WithName("GetUserRoles")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


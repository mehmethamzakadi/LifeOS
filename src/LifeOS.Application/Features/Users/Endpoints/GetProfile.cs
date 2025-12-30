using LifeOS.Application.Abstractions;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class GetProfile
{
    public sealed record Response(
        Guid Id,
        string UserName,
        string Email,
        string? PhoneNumber,
        string? ProfilePictureUrl,
        bool EmailConfirmed,
        DateTime CreatedDate);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/profile", async (
            LifeOSDbContext context,
            ICurrentUserService currentUserService,
            CancellationToken cancellationToken) =>
        {
            var userId = currentUserService.GetCurrentUserId();
            if (userId == null)
            {
                return Results.Unauthorized();
            }

            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                return Results.NotFound(new { Error = "Kullanıcı bulunamadı." });
            }

            var response = new Response(
                user.Id,
                user.UserName,
                user.Email,
                user.PhoneNumber,
                user.ProfilePictureUrl,
                user.EmailConfirmed,
                user.CreatedDate);

            return Results.Ok(response);
        })
        .WithName("GetProfile")
        .WithTags("Profile")
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    }
}


using AutoMapper;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class GetUserById
{
    public sealed record Response(
        Guid Id,
        string UserName,
        string Email,
        DateTimeOffset? LockoutEnd,
        bool LockoutEnabled,
        int AccessFailedCount);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/{id}", async (
            Guid id,
            LifeOSDbContext context,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var user = await context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

            if (user is null)
                return Results.NotFound(new { Error = "Kullanıcı bulunamadı!" });

            var response = mapper.Map<Response>(user);
            return Results.Ok(response);
        })
        .WithName("GetUserById")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


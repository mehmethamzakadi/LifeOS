using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.Endpoints;

public static class GetAllPermissions
{
    public sealed record PermissionDto(
        Guid Id,
        string Name,
        string Description,
        string Type);

    public sealed record PermissionModuleDto(
        string ModuleName,
        List<PermissionDto> Permissions);

    public sealed record Response(List<PermissionModuleDto> Modules);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/permissions", async (
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var permissions = await context.Permissions
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Type)
                .ToListAsync(cancellationToken);

            var grouped = permissions
                .GroupBy(p => p.Module)
                .Select(g => new PermissionModuleDto(
                    g.Key,
                    g.Select(p => new PermissionDto(
                        p.Id,
                        p.Name,
                        p.Description ?? string.Empty,
                        p.Type)).ToList()))
                .OrderBy(m => m.ModuleName)
                .ToList();

            return Results.Ok(new Response(grouped));
        })
        .WithName("GetAllPermissions")
        .WithTags("Permissions")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesRead)
        .Produces<Response>(StatusCodes.Status200OK);
    }
}


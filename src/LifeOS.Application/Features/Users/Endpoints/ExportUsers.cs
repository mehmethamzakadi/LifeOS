using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class ExportUsers
{
    public sealed record Request(string Format = "csv");

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/users/export", async (
            [AsParameters] Request request,
            LifeOSDbContext context,
            CancellationToken cancellationToken) =>
        {
            var users = await context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .OrderBy(u => u.Id)
                .ToListAsync(cancellationToken);

            var csv = GenerateCsv(users);
            var bytes = Encoding.UTF8.GetBytes(csv);

            return Results.File(
                bytes,
                "text/csv",
                $"users_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        })
        .WithName("ExportUsers")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersViewAll)
        .Produces(StatusCodes.Status200OK);
    }

    private static string GenerateCsv(List<User> users)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,UserName,Email,PhoneNumber,EmailConfirmed");

        foreach (var user in users)
        {
            sb.AppendLine($"{user.Id},{EscapeCsv(user.UserName)},{EscapeCsv(user.Email)},{EscapeCsv(user.PhoneNumber)},{user.EmailConfirmed}");
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}


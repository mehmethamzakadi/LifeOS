using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LifeOS.Application.Features.Users.ExportUsers;

public sealed class ExportUsersHandler
{
    private readonly LifeOSDbContext _context;

    public ExportUsersHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)> HandleAsync(
        string format,
        CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        var csv = GenerateCsv(users);
        var bytes = Encoding.UTF8.GetBytes(csv);

        return (bytes, "text/csv", $"users_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
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


using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LifeOS.Application.Features.Users.Queries.Export;

public class ExportUsersQueryHandler : IRequestHandler<ExportUsersQuery, ExportUsersResponse>
{
    private readonly LifeOSDbContext _context;

    public ExportUsersQueryHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ExportUsersResponse> Handle(ExportUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        var csv = GenerateCsv(users);
        var bytes = Encoding.UTF8.GetBytes(csv);

        return new ExportUsersResponse
        {
            FileContent = bytes,
            FileName = $"users_{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv"
        };
    }

    private string GenerateCsv(List<User> users)
    {
        var sb = new StringBuilder();

        // Başlık
        sb.AppendLine("Id,UserName,Email,PhoneNumber,EmailConfirmed");

        // Satırlar
        foreach (var user in users)
        {
            sb.AppendLine($"{user.Id},{EscapeCsv(user.UserName.Value)},{EscapeCsv(user.Email.Value)},{EscapeCsv(user.PhoneNumber)},{user.EmailConfirmed}");
        }

        return sb.ToString();
    }

    private string EscapeCsv(string? value)
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

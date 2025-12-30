using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Queries.GetUserRoles;

public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, IDataResult<GetUserRolesResponse>>
{
    private readonly LifeOSDbContext _context;

    public GetUserRolesQueryHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<IDataResult<GetUserRolesResponse>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return new ErrorDataResult<GetUserRolesResponse>("Kullanıcı bulunamadı");
        }

        // Rolleri user.UserRoles üzerinden direkt alıyoruz (zaten include edildi)
        var userRoles = user.UserRoles
            .Select(ur => new UserRoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name ?? string.Empty
            })
            .ToList();

        var response = new GetUserRolesResponse
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = userRoles
        };

        return new SuccessDataResult<GetUserRolesResponse>(response, "Kullanıcı rolleri başarıyla getirildi");
    }
}

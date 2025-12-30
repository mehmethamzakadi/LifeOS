using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.Queries.GetRolePermissions;

public class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, IDataResult<GetRolePermissionsResponse>>
{
    private readonly LifeOSDbContext _context;

    public GetRolePermissionsQueryHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<IDataResult<GetRolePermissionsResponse>> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var role = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted, cancellationToken);

        if (role == null)
        {
            return new ErrorDataResult<GetRolePermissionsResponse>("Rol bulunamadı");
        }

        var permissionIds = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == request.RoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var response = new GetRolePermissionsResponse
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            PermissionIds = permissionIds
        };

        return new SuccessDataResult<GetRolePermissionsResponse>(response, "Rol permission'ları başarıyla getirildi");
    }
}

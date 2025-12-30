using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.GetRolePermissions;

public sealed class GetRolePermissionsHandler
{
    private readonly LifeOSDbContext _context;

    public GetRolePermissionsHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetRolePermissionsResponse>> HandleAsync(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

        if (role == null)
            return ApiResultExtensions.Failure<GetRolePermissionsResponse>("Rol bulunamadı");

        var permissionIds = await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        var response = new GetRolePermissionsResponse(
            role.Id,
            role.Name ?? string.Empty,
            permissionIds);

        return ApiResultExtensions.Success(response, "Rol izinleri başarıyla getirildi");
    }
}


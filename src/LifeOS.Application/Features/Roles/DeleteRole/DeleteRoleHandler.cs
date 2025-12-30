using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.DeleteRole;

public sealed class DeleteRoleHandler
{
    private readonly LifeOSDbContext _context;

    public DeleteRoleHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, cancellationToken);

        if (role == null)
            return ApiResultExtensions.Failure(ResponseMessages.Role.NotFound);

        if (role.NormalizedName == "ADMIN")
            return ApiResultExtensions.Failure("Admin rolü silinemez!");

        if (role.UserRoles.Any(ur => !ur.IsDeleted))
            return ApiResultExtensions.Failure("Bu role atanmış aktif kullanıcılar bulunmaktadır. Önce kullanıcılardan bu rolü kaldırmalısınız.");

        role.Delete();
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(ResponseMessages.Role.Deleted);
    }
}


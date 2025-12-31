using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.BulkDeleteRoles;

public sealed class BulkDeleteRolesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteRolesHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<BulkDeleteRolesResponse>> HandleAsync(
        BulkDeleteRolesCommand command,
        CancellationToken cancellationToken)
    {
        var deletedCount = 0;
        var failedCount = 0;
        var errors = new List<string>();

        foreach (var roleId in command.RoleIds)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.UserRoles)
                        .ThenInclude(ur => ur.User)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

                if (role == null)
                {
                    errors.Add($"Rol bulunamadı: ID {roleId}");
                    failedCount++;
                    continue;
                }

                if (role.NormalizedName == "ADMIN")
                {
                    errors.Add($"Admin rolü silinemez");
                    failedCount++;
                    continue;
                }

                // Sadece aktif (silinmemiş) kullanıcılara atanmış rolleri kontrol et
                if (role.UserRoles.Any(ur => !ur.IsDeleted && ur.User != null && !ur.User.IsDeleted))
                {
                    errors.Add($"'{role.Name}' rolüne atanmış aktif kullanıcılar bulunmaktadır");
                    failedCount++;
                    continue;
                }

                role.Delete();
                _context.Roles.Update(role);
                deletedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Rol silinirken hata oluştu (ID {roleId}): {ex.Message}");
                failedCount++;
            }
        }

        var response = new BulkDeleteRolesResponse(deletedCount, failedCount, errors);

        if (deletedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        var message = failedCount > 0 
            ? $"{deletedCount} rol silindi, {failedCount} rol silinemedi"
            : $"{deletedCount} rol başarıyla silindi";

        return ApiResultExtensions.Success(response, message);
    }
}


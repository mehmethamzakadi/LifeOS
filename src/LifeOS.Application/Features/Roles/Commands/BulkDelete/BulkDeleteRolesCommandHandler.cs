using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Commands.BulkDelete;

public class BulkDeleteRolesCommandHandler : IRequestHandler<BulkDeleteRolesCommand, BulkDeleteRolesResponse>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteRolesCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BulkDeleteRolesResponse> Handle(BulkDeleteRolesCommand request, CancellationToken cancellationToken)
    {
        var response = new BulkDeleteRolesResponse();

        foreach (var roleId in request.RoleIds)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.UserRoles)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

                if (role == null)
                {
                    response.Errors.Add($"Rol bulunamadı: ID {roleId}");
                    response.FailedCount++;
                    continue;
                }

                if (role.NormalizedName == "ADMIN")
                {
                    response.Errors.Add($"Admin rolü silinemez");
                    response.FailedCount++;
                    continue;
                }

                if (role.UserRoles.Any(ur => !ur.IsDeleted))
                {
                    response.Errors.Add($"'{role.Name}' rolüne atanmış aktif kullanıcılar bulunmaktadır");
                    response.FailedCount++;
                    continue;
                }

                role.Delete();
                _context.Roles.Update(role);
                response.DeletedCount++;
            }
            catch (Exception ex)
            {
                response.Errors.Add($"Rol silinirken hata oluştu (ID {roleId}): {ex.Message}");
                response.FailedCount++;
            }
        }

        if (response.DeletedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
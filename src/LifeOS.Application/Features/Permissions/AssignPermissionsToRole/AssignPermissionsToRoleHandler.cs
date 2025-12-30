using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Events.PermissionEvents;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.AssignPermissionsToRole;

public sealed class AssignPermissionsToRoleHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignPermissionsToRoleHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid roleId,
        AssignPermissionsToRoleCommand command,
        CancellationToken cancellationToken)
    {
        if (roleId != command.RoleId)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == command.RoleId && !r.IsDeleted, cancellationToken);

        if (role == null)
            return ApiResultExtensions.Failure("Rol bulunamadı");

        var permissionsEntities = await _context.Permissions
            .AsNoTracking()
            .Where(p => command.PermissionIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);
        var permissions = permissionsEntities.Select(p => p.Name).ToList();

        var existingRolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == command.RoleId)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = existingRolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var requestedPermissionIds = command.PermissionIds.ToHashSet();

        var permissionsToRemove = existingPermissionIds.Except(requestedPermissionIds).ToList();
        var permissionsToAdd = requestedPermissionIds.Except(existingPermissionIds).ToList();

        if (permissionsToRemove.Any())
        {
            var rolePermissionsToRemove = existingRolePermissions
                .Where(rp => permissionsToRemove.Contains(rp.PermissionId))
                .ToList();

            _context.RolePermissions.RemoveRange(rolePermissionsToRemove);
        }

        if (permissionsToAdd.Any())
        {
            foreach (var permissionId in permissionsToAdd)
            {
                var newRolePermission = new RolePermission
                {
                    RoleId = command.RoleId,
                    PermissionId = permissionId
                };
                await _context.RolePermissions.AddAsync(newRolePermission, cancellationToken);
            }
        }

        role.AddDomainEvent(new PermissionsAssignedToRoleEvent(role.Id, role.Name!, permissions));

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(ResponseMessages.Permission.Assigned);
    }
}


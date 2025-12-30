using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.AssignRolesToUser;

public sealed class AssignRolesToUserHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignRolesToUserHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        AssignRolesToUserCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.UserId)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId && !u.IsDeleted, cancellationToken);
        if (user == null)
            return ApiResultExtensions.Failure("Kullanıcı bulunamadı");

        var requestedRoleIds = command.RoleIds.ToHashSet();

        var existingUserRoles = await _context.UserRoles
            .IgnoreQueryFilters()
            .Where(ur => ur.UserId == command.UserId)
            .ToListAsync(cancellationToken);

        var existingRoleIds = existingUserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => ur.RoleId)
            .ToHashSet();

        var rolesToRemove = existingRoleIds.Except(requestedRoleIds).ToList();
        var rolesToAdd = requestedRoleIds.Except(existingRoleIds).ToList();

        if (!rolesToRemove.Any() && !rolesToAdd.Any())
        {
            return ApiResultExtensions.Success("Roller zaten güncel");
        }

        if (rolesToRemove.Any())
        {
            var userRolesToRemove = existingUserRoles
                .Where(ur => rolesToRemove.Contains(ur.RoleId) && !ur.IsDeleted)
                .ToList();

            foreach (var userRole in userRolesToRemove)
            {
                userRole.Delete();
                _context.UserRoles.Update(userRole);
            }
        }

        if (rolesToAdd.Any())
        {
            foreach (var roleId in rolesToAdd)
            {
                var deletedUserRole = existingUserRoles
                    .FirstOrDefault(ur => ur.RoleId == roleId && ur.IsDeleted);

                if (deletedUserRole != null)
                {
                    deletedUserRole.Restore();
                    _context.UserRoles.Update(deletedUserRole);
                }
                else
                {
                    var newUserRole = new UserRole
                    {
                        UserId = command.UserId,
                        RoleId = roleId
                    };
                    await _context.UserRoles.AddAsync(newUserRole, cancellationToken);
                }
            }
        }

        var currentRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == command.UserId && !ur.IsDeleted)
            .Select(ur => ur.Role.Name!)
            .ToListAsync(cancellationToken);

        user.AddDomainEvent(new UserRolesAssignedEvent(user.Id, user.UserName, currentRoles));

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success("Roller başarıyla atandı");
    }
}


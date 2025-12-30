using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Events.PermissionEvents;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Permissions.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionsToRoleCommandHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        // Rol kontrolü
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId && !r.IsDeleted, cancellationToken);
            
        if (role == null)
        {
            return new ErrorResult("Rol bulunamadı");
        }

        // Permission bilgilerini al (event için)
        var permissionsEntities = await _context.Permissions
            .AsNoTracking()
            .Where(p => request.PermissionIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync(cancellationToken);
        var permissions = permissionsEntities.Select(p => p.Name).ToList();

        // Mevcut RolePermission'ları al
        var existingRolePermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == request.RoleId)
            .ToListAsync(cancellationToken);

        var existingPermissionIds = existingRolePermissions
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var requestedPermissionIds = request.PermissionIds.ToHashSet();

        // Silinecek permission'lar (mevcut ama istenen listede yok)
        var permissionsToRemove = existingPermissionIds.Except(requestedPermissionIds).ToList();

        // Eklenecek permission'lar (istenen listede var ama mevcut değil)
        var permissionsToAdd = requestedPermissionIds.Except(existingPermissionIds).ToList();

        // Silinecek permission'ları fiziksel olarak sil (RolePermission composite key kullanıyor, soft delete yok)
        if (permissionsToRemove.Any())
        {
            var rolePermissionsToRemove = existingRolePermissions
                .Where(rp => permissionsToRemove.Contains(rp.PermissionId))
                .ToList();

            _context.RolePermissions.RemoveRange(rolePermissionsToRemove);
        }

        // Eklenecek permission'ları ekle
        if (permissionsToAdd.Any())
        {
            foreach (var permissionId in permissionsToAdd)
            {
                var newRolePermission = new RolePermission
                {
                    RoleId = request.RoleId,
                    PermissionId = permissionId
                };
                await _context.RolePermissions.AddAsync(newRolePermission, cancellationToken);
            }
        }

        // Domain event ekle
        role.AddDomainEvent(new PermissionsAssignedToRoleEvent(role.Id, role.Name!, permissions));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Permission'lar başarıyla atandı");
    }
}
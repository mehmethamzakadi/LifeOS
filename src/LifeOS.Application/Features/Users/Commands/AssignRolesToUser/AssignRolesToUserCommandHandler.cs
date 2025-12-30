using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Users.Commands.AssignRolesToUser;

public class AssignRolesToUserCommandHandler : IRequestHandler<AssignRolesToUserCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRolesToUserCommandHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(AssignRolesToUserCommand request, CancellationToken cancellationToken)
    {
        // Kullanıcı kontrolü
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && !u.IsDeleted, cancellationToken);
        if (user == null)
        {
            return new ErrorResult("Kullanıcı bulunamadı");
        }

        var requestedRoleIds = request.RoleIds.ToHashSet();

        // Mevcut UserRole'leri al (silinmiş olanlar dahil - IgnoreQueryFilters ile)
        var existingUserRoles = await _context.UserRoles
            .IgnoreQueryFilters()
            .Where(ur => ur.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var existingRoleIds = existingUserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => ur.RoleId)
            .ToHashSet();

        // Silinecek roller (mevcut ama istenen listede yok)
        var rolesToRemove = existingRoleIds.Except(requestedRoleIds).ToList();

        // Eklenecek roller (istenen listede var ama mevcut değil)
        var rolesToAdd = requestedRoleIds.Except(existingRoleIds).ToList();

        // Değişiklik yoksa erken çık
        if (!rolesToRemove.Any() && !rolesToAdd.Any())
        {
            return new SuccessResult("Roller zaten güncel");
        }

        // Silinecek rolleri soft delete yap
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

        // Eklenecek rolleri ekle veya geri ekle
        if (rolesToAdd.Any())
        {
            foreach (var roleId in rolesToAdd)
            {
                // Önce silinmiş bir UserRole var mı kontrol et
                var deletedUserRole = existingUserRoles
                    .FirstOrDefault(ur => ur.RoleId == roleId && ur.IsDeleted);

                if (deletedUserRole != null)
                {
                    // Silinmiş rolü geri ekle (soft delete'i kaldır)
                    deletedUserRole.Restore();
                    _context.UserRoles.Update(deletedUserRole);
                }
                else
                {
                    // Yeni UserRole oluştur
                    var newUserRole = new UserRole
                    {
                        UserId = request.UserId,
                        RoleId = roleId
                    };
                    await _context.UserRoles.AddAsync(newUserRole, cancellationToken);
                }
            }
        }

        // Domain Event - Rolleri context'ten al
        var currentRoles = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == request.UserId && !ur.IsDeleted)
            .Select(ur => ur.Role.Name!)
            .ToListAsync(cancellationToken);
        
        user.AddDomainEvent(new UserRolesAssignedEvent(user.Id, user.UserName.Value, currentRoles));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Roller başarıyla atandı");
    }
}
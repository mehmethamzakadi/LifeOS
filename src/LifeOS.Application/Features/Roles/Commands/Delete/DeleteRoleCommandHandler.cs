using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Roles.Commands.Delete;

public sealed class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteRoleCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (role == null)
            return new ErrorResult("Rol bulunamadı!");

        if (role.NormalizedName == "ADMIN")
            return new ErrorResult("Admin rolü silinemez!");

        if (role.UserRoles.Any(ur => !ur.IsDeleted))
            return new ErrorResult("Bu role atanmış aktif kullanıcılar bulunmaktadır. Önce kullanıcılardan bu rolü kaldırmalısınız.");

        role.Delete();
        _context.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Rol silindi.");
    }
}

using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Roles.Commands.Update;

public sealed class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRoleCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);
            
        if (role == null)
            return new ErrorResult("Rol bulunamadı!");

        var normalizedName = request.Name.ToUpperInvariant();
        var existingRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        if (existingRole != null && existingRole.Id != request.Id)
            return new ErrorResult($"Güncellemek istediğiniz {request.Name} rolü sistemde mevcut!");

        // ✅ RICH DOMAIN: Update method internally updates ConcurrencyStamp
        role.Update(request.Name);
        _context.Roles.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Rol güncellendi.");
    }
}

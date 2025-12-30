using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Roles.Commands.Create;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.ToUpperInvariant();
        var checkRole = await _context.Roles
            .AnyAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        if (checkRole)
            return new ErrorResult("Eklemek istediğiniz Rol sistemde mevcut!");

        var role = Role.Create(request.Name);
        await _context.Roles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Rol oluşturuldu.");
    }
}

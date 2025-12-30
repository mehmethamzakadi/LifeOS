using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Users.Commands.Delete;

public sealed class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

        if (user == null)
            return new ErrorResult("Kullanıcı bilgisi bulunamadı!");

        user.Delete();
        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Kullanıcı bilgisi başarıyla silindi.");
    }
}

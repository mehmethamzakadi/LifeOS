using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Users.Commands.Update;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);
        if (user is null)
            return new ErrorResult("Kullanıcı Bilgisi Bulunamadı!");

        if (user.Email != request.Email)
        {
            var existingEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
            if (existingEmail != null && existingEmail.Id != request.Id)
                return new ErrorResult("Bu e-posta adresi zaten kullanılıyor!");
        }

        if (user.UserName != request.UserName)
        {
            var existingUserName = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);
            if (existingUserName != null && existingUserName.Id != request.Id)
                return new ErrorResult("Bu kullanıcı adı zaten kullanılıyor!");
        }

        user.Update(request.UserName, request.Email);
        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Kullanıcı bilgisi başarıyla güncellendi.");
    }
}

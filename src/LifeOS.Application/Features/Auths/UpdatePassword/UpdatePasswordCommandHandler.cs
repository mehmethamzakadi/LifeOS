using LifeOS.Domain.Common;
using LifeOS.Domain.Exceptions;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Auths.UpdatePassword;

public sealed class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, UpdatePasswordResponse>
{
    private readonly LifeOSDbContext _context;
    private readonly IUserDomainService _userDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePasswordCommandHandler(
        LifeOSDbContext context,
        IUserDomainService userDomainService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _userDomainService = userDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdatePasswordResponse> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!request.Password.Equals(request.PasswordConfirm))
            throw new PasswordChangeFailedException("Girilen şifre aynı değil, lütfen şifreyi doğrulayınız!");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(request.UserId) && !u.IsDeleted, cancellationToken);
        if (user == null)
            throw new PasswordChangeFailedException("Kullanıcı bulunamadı.");

        var result = _userDomainService.ResetPassword(user, request.ResetToken, request.Password);
        if (!result.Success)
            throw new PasswordChangeFailedException(result.Message ?? "Şifre güncellenemedi.");

        _context.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new();
    }
}

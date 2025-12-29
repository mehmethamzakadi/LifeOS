using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using LifeOS.Domain.Services;
using MediatR;

namespace LifeOS.Application.Features.Users.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IUserDomainService userDomainService,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangePasswordCommand, IResult>
{
    public async Task<IResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return new ErrorResult("Kullanıcı kimliği bulunamadı.");
        }

        var user = await userRepository.FindByIdAsync(userId.Value);
        if (user == null)
        {
            return new ErrorResult("Kullanıcı bulunamadı.");
        }

        // Mevcut şifreyi doğrula
        if (!userDomainService.VerifyPassword(user, request.CurrentPassword))
        {
            return new ErrorResult("Mevcut şifre hatalı.");
        }

        // Yeni şifreyi ayarla
        var result = userDomainService.SetPassword(user, request.NewPassword);
        if (!result.Success)
        {
            return result;
        }

        // User zaten tracking'de olduğu için Update çağrısına gerek yok
        // EF Core değişiklikleri otomatik algılayacak
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Şifre başarıyla değiştirildi.");
    }
}

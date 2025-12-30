using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Commands.UpdateCurrentUserProfile;

public sealed class UpdateCurrentUserProfileCommandHandler(
    ICurrentUserService currentUserService,
    LifeOSDbContext context,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCurrentUserProfileCommand, IResult>
{
    public async Task<IResult> Handle(UpdateCurrentUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return new ErrorResult("Kullanıcı kimliği bulunamadı.");
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);
        if (user == null)
        {
            return new ErrorResult("Kullanıcı bulunamadı.");
        }

        // Email değiştiyse kontrol et
        if (user.Email.Value != request.Email)
        {
            var existingEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);
                
            if (existingEmail != null && existingEmail.Id != userId.Value)
            {
                return new ErrorResult("Bu e-posta adresi zaten kullanılıyor!");
            }
        }

        // UserName değiştiyse kontrol et
        if (user.UserName.Value != request.UserName)
        {
            var existingUserName = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName.Value == request.UserName, cancellationToken);
            if (existingUserName != null && existingUserName.Id != userId.Value)
            {
                return new ErrorResult("Bu kullanıcı adı zaten kullanılıyor!");
            }
        }

        // Kullanıcı bilgilerini güncelle
        user.Update(request.UserName, request.Email);
        user.UpdateProfile(request.PhoneNumber, request.ProfilePictureUrl);

        // User zaten tracking'de olduğu için Update çağrısına gerek yok
        // EF Core değişiklikleri otomatik algılayacak
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Profil bilgileri başarıyla güncellendi.");
    }
}

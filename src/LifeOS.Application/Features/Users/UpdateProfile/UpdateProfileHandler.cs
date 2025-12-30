using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.UpdateProfile;

public sealed class UpdateProfileHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProfileHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure("Yetkisiz erişim");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);
        if (user == null)
        {
            return ApiResultExtensions.Failure("Kullanıcı bulunamadı.");
        }

        if (user.Email != command.Email)
        {
            var existingEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

            if (existingEmail != null && existingEmail.Id != userId.Value)
            {
                return ApiResultExtensions.Failure("Bu e-posta adresi zaten kullanılıyor!");
            }
        }

        if (user.UserName != command.UserName)
        {
            var existingUserName = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == command.UserName, cancellationToken);
            if (existingUserName != null && existingUserName.Id != userId.Value)
            {
                return ApiResultExtensions.Failure("Bu kullanıcı adı zaten kullanılıyor!");
            }
        }

        user.Update(command.UserName, command.Email);
        user.UpdateProfile(command.PhoneNumber, command.ProfilePictureUrl);

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success("Profil başarıyla güncellendi");
    }
}


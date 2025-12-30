using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserDomainService _userDomainService;

    public ChangePasswordHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService,
        IUserDomainService userDomainService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _userDomainService = userDomainService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        ChangePasswordCommand command,
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

        if (!_userDomainService.VerifyPassword(user, command.CurrentPassword))
        {
            return ApiResultExtensions.Failure("Mevcut şifre hatalı.");
        }

        var result = _userDomainService.SetPassword(user, command.NewPassword);
        if (!result.Success)
        {
            return ApiResultExtensions.Failure(result.Message ?? "Şifre ayarlanamadı");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success("Şifre başarıyla değiştirildi.");
    }
}


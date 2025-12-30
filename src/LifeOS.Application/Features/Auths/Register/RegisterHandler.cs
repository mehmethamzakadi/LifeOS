using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Auths.Register;

public sealed class RegisterHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IUserDomainService _userDomainService;

    public RegisterHandler(LifeOSDbContext context, IUserDomainService userDomainService)
    {
        _context = context;
        _userDomainService = userDomainService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        
        if (existingUser is not null)
        {
            return ApiResultExtensions.Failure("Bu e-posta adresi zaten kullanılıyor!");
        }

        var user = User.Create(command.UserName, command.Email, string.Empty);

        var passwordResult = _userDomainService.SetPassword(user, command.Password);
        if (!passwordResult.Success)
            return ApiResultExtensions.Failure(passwordResult.Message ?? "Şifre ayarlanamadı");

        await _context.Users.AddAsync(user, cancellationToken);

        var userRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName.Equals(UserRoles.User, StringComparison.InvariantCultureIgnoreCase) && !r.IsDeleted, cancellationToken);
        
        if (userRole != null)
        {
            var roleResult = _userDomainService.AddToRole(user, userRole);
            if (!roleResult.Success)
                return ApiResultExtensions.Failure(roleResult.Message ?? "Rol atanamadı");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResultExtensions.Success("Kayıt işlemi başarılı. Giriş yapabilirsiniz.");
    }
}


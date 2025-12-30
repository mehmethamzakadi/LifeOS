using LifeOS.Domain.Common.Results;

namespace LifeOS.Application.Abstractions.Identity;

public interface IAuthService
{
    Task<IDataResult<AuthResult>> LoginAsync(string email, string password, string? deviceId = null);
    Task<IDataResult<AuthResult>> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task PasswordResetAsync(string email);
    Task<IDataResult<bool>> PasswordVerify(string resetToken, string userId);
}

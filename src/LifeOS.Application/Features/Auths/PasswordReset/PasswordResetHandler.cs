using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;

namespace LifeOS.Application.Features.Auths.PasswordReset;

public sealed class PasswordResetHandler
{
    private readonly IAuthService _authService;

    public PasswordResetHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        PasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        await _authService.PasswordResetAsync(command.Email);
        return ApiResultExtensions.Success("Şifre yenileme işlemleri için mail gönderildi.");
    }
}


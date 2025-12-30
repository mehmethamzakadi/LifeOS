using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;

namespace LifeOS.Application.Features.Auths.PasswordVerify;

public sealed class PasswordVerifyHandler
{
    private readonly IAuthService _authService;

    public PasswordVerifyHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResult<PasswordVerifyResponse>> HandleAsync(
        PasswordVerifyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _authService.PasswordVerify(command.ResetToken, command.UserId);
        var responseData = new PasswordVerifyResponse(result.Data, result.Message ?? "Token doğrulandı");
        return ApiResultExtensions.Success(responseData, result.Message ?? "Token doğrulandı");
    }
}


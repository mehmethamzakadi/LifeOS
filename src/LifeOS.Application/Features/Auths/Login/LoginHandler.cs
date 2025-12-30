using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.Auths.Common;
using Microsoft.AspNetCore.Http;

namespace LifeOS.Application.Features.Auths.Login;

public sealed class LoginHandler
{
    private readonly IAuthService _authService;

    public LoginHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResult<LoginResponse>> HandleAsync(
        LoginCommand command,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(command.Email, command.Password, command.DeviceId);

        if (!result.Success || result.Data is null)
        {
            AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
            return ApiResultExtensions.Failure<LoginResponse>("Giriş başarısız. E-posta veya şifre hatalı.");
        }

        AuthCookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

        var responseData = new LoginResponse(
            result.Data.UserId,
            result.Data.UserName,
            result.Data.Expiration,
            result.Data.Token,
            result.Data.Permissions);

        return ApiResultExtensions.Success(responseData, "Giriş başarılı");
    }
}


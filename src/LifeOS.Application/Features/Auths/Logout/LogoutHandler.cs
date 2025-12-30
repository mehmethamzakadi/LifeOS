using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.Auths.Common;
using Microsoft.AspNetCore.Http;

namespace LifeOS.Application.Features.Auths.Logout;

public sealed class LogoutHandler
{
    private readonly IAuthService _authService;

    public LogoutHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetRefreshTokenFromCookie(httpContext, out var refreshToken))
        {
            AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
            return ApiResultExtensions.Success("Çıkış yapıldı.");
        }

        await _authService.LogoutAsync(refreshToken);
        AuthCookieHelper.ClearRefreshTokenCookie(httpContext);

        return ApiResultExtensions.Success("Çıkış yapıldı.");
    }
}


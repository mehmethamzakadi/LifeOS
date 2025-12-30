using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.Auths.Common;
using Microsoft.AspNetCore.Http;

namespace LifeOS.Application.Features.Auths.RefreshToken;

public sealed class RefreshTokenHandler
{
    private readonly IAuthService _authService;

    public RefreshTokenHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResult<RefreshTokenResponse>> HandleAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetRefreshTokenFromCookie(httpContext, out var refreshToken))
        {
            AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
            return ApiResultExtensions.Failure<RefreshTokenResponse>("Geçersiz refresh token");
        }

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (!result.Success || result.Data is null)
        {
            AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
            return ApiResultExtensions.Failure<RefreshTokenResponse>("Token yenileme başarısız");
        }

        AuthCookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

        var responseData = new RefreshTokenResponse(
            result.Data.UserId,
            result.Data.UserName,
            result.Data.Expiration,
            result.Data.Token,
            result.Data.Permissions);

        return ApiResultExtensions.Success(responseData, "Token başarıyla yenilendi");
    }
}


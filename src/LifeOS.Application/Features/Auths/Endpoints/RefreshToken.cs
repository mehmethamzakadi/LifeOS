using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class RefreshToken
{
    public sealed record Response(
        Guid UserId,
        string UserName,
        DateTime Expiration,
        string Token,
        List<string> Permissions);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh-token", async (
            HttpContext httpContext,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            if (!AuthCookieHelper.TryGetRefreshTokenFromCookie(httpContext, out var refreshToken))
            {
                AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
                return ApiResultExtensions.Failure<Response>("Geçersiz refresh token").ToResult();
            }

            var result = await authService.RefreshTokenAsync(refreshToken);

            if (!result.Success || result.Data is null)
            {
                AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
                return ApiResultExtensions.Failure<Response>("Token yenileme başarısız").ToResult();
            }

            AuthCookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken, result.Data.RefreshTokenExpiration);

            var responseData = new Response(
                result.Data.UserId,
                result.Data.UserName,
                result.Data.Expiration,
                result.Data.Token,
                result.Data.Permissions);

            return ApiResultExtensions.Success(responseData, "Token başarıyla yenilendi").ToResult();
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<Response>>(StatusCodes.Status200OK)
        .Produces<ApiResult<Response>>(StatusCodes.Status401Unauthorized);
    }
}


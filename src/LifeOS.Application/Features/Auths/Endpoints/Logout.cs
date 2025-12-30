using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class Logout
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/logout", async (
            HttpContext httpContext,
            IAuthService authService,
            CancellationToken cancellationToken) =>
        {
            if (!AuthCookieHelper.TryGetRefreshTokenFromCookie(httpContext, out var refreshToken))
            {
                AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
                return ApiResultExtensions.Success("Çıkış yapıldı.").ToResult();
            }

            await authService.LogoutAsync(refreshToken);
            AuthCookieHelper.ClearRefreshTokenCookie(httpContext);

            return ApiResultExtensions.Success("Çıkış yapıldı.").ToResult();
        })
        .WithName("Logout")
        .WithTags("Auth")
        .Produces<ApiResult<object>>(StatusCodes.Status200OK);
    }
}


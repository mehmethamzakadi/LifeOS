using LifeOS.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class Logout
{
    public sealed record Response(bool Success, string Message);

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
                return Results.Ok(new Response(true, "Çıkış yapıldı."));
            }

            await authService.LogoutAsync(refreshToken);
            AuthCookieHelper.ClearRefreshTokenCookie(httpContext);

            return Results.Ok(new Response(true, "Çıkış yapıldı."));
        })
        .WithName("Logout")
        .WithTags("Auth")
        .Produces<Response>(StatusCodes.Status200OK);
    }
}


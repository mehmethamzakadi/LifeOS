using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LifeOS.Application.Features.Auths.Common;

public static class AuthCookieHelper
{
    private const string RefreshTokenCookieName = "baseproject_refresh_token";

    public static void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken, DateTime refreshTokenExpiration)
    {
        var options = CreateRefreshTokenCookieOptions(httpContext, refreshTokenExpiration);
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, options);
    }

    public static void ClearRefreshTokenCookie(HttpContext httpContext)
    {
        var options = CreateRefreshTokenCookieOptions(httpContext, DateTime.UtcNow.AddYears(-1));
        httpContext.Response.Cookies.Append(RefreshTokenCookieName, string.Empty, options);
    }

    public static bool TryGetRefreshTokenFromCookie(HttpContext httpContext, out string refreshToken)
    {
        if (httpContext.Request.Cookies.TryGetValue(RefreshTokenCookieName, out var token) && !string.IsNullOrWhiteSpace(token))
        {
            refreshToken = token;
            return true;
        }

        refreshToken = string.Empty;
        return false;
    }

    private static CookieOptions CreateRefreshTokenCookieOptions(HttpContext httpContext, DateTime refreshExpirationUtc)
    {
        var environment = httpContext.RequestServices.GetService<IWebHostEnvironment>();
        var isDevelopment = environment?.EnvironmentName == Environments.Development;

        var host = httpContext.Request.Host.Host;
        string? cookieDomain = null;

        if (!string.IsNullOrWhiteSpace(host) && host.Contains('.'))
        {
            cookieDomain = host;
        }

        var requestIsHttps = httpContext.Request.IsHttps;

        var sameSite = SameSiteMode.Strict;
        var secure = requestIsHttps;

        if (httpContext.Request.Headers.TryGetValue("Origin", out var originHeader) &&
            Uri.TryCreate(originHeader.ToString(), UriKind.Absolute, out var originUri) &&
            !string.Equals(originUri.Host, host, StringComparison.OrdinalIgnoreCase))
        {
            if (requestIsHttps)
            {
                sameSite = SameSiteMode.None;
                secure = true;
            }
            else
            {
                sameSite = SameSiteMode.Lax;
            }
        }
        else if (isDevelopment && !requestIsHttps)
        {
            sameSite = SameSiteMode.Lax;
        }

        if (!requestIsHttps && !isDevelopment)
        {
            secure = false;
        }

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = "/",
            Expires = new DateTimeOffset(DateTime.SpecifyKind(refreshExpirationUtc, DateTimeKind.Utc)),
            Domain = cookieDomain
        };
    }
}


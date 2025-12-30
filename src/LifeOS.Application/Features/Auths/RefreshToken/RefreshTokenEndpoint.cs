using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.RefreshToken;

public static class RefreshTokenEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh-token", async (
            HttpContext httpContext,
            RefreshTokenHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(httpContext, cancellationToken);
            return result.ToResult();
        })
        .WithName("RefreshToken")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<RefreshTokenResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<RefreshTokenResponse>>(StatusCodes.Status401Unauthorized);
    }
}


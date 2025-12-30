using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Logout;

public static class LogoutEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/logout", async (
            HttpContext httpContext,
            LogoutHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(httpContext, cancellationToken);
            return result.ToResult();
        })
        .WithName("Logout")
        .WithTags("Auth")
        .Produces<ApiResult<object>>(StatusCodes.Status200OK);
    }
}


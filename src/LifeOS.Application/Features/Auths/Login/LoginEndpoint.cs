using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.Auths.Common;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Login;

public static class LoginEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/login", async (
            LoginCommand command,
            LoginHandler handler,
            IValidator<LoginCommand> validator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                AuthCookieHelper.ClearRefreshTokenCookie(httpContext);
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(command, httpContext, cancellationToken);
            return result.ToResult();
        })
        .WithName("Login")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<LoginResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}


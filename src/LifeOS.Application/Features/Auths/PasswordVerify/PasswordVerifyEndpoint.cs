using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.PasswordVerify;

public static class PasswordVerifyEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/password-verify", async (
            PasswordVerifyCommand command,
            PasswordVerifyHandler handler,
            IValidator<PasswordVerifyCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("PasswordVerify")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<PasswordVerifyResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<PasswordVerifyResponse>>(StatusCodes.Status400BadRequest);
    }
}


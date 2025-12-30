using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.PasswordReset;

public static class PasswordResetEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/password-reset", async (
            PasswordResetCommand command,
            PasswordResetHandler handler,
            IValidator<PasswordResetCommand> validator,
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
        .WithName("PasswordReset")
        .WithTags("Auth")
        .AllowAnonymous()
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


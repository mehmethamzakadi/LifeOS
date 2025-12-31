using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GamePlatforms.UpdateGamePlatform;

public static class UpdateGamePlatformEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/game-platforms/{id:guid}", async (
            Guid id,
            UpdateGamePlatformCommand command,
            UpdateGamePlatformHandler handler,
            IValidator<UpdateGamePlatformCommand> validator,
            CancellationToken cancellationToken) =>
        {
            command = command with { Id = id };

            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("UpdateGamePlatform")
        .WithTags("GamePlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.GamePlatformsUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


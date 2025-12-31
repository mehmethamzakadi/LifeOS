using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GamePlatforms.CreateGamePlatform;

public static class CreateGamePlatformEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/game-platforms", async (
            CreateGamePlatformCommand command,
            CreateGamePlatformHandler handler,
            IValidator<CreateGamePlatformCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            try
            {
                var response = await handler.HandleAsync(command, cancellationToken);
                return ApiResultExtensions.CreatedResult(
                    response,
                    $"/api/game-platforms/{response.Id}",
                    "Oyun platformu başarıyla oluşturuldu");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateGamePlatformResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateGamePlatform")
        .WithTags("GamePlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.GamePlatformsCreate)
        .Produces<ApiResult<CreateGamePlatformResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateGamePlatformResponse>>(StatusCodes.Status400BadRequest);
    }
}


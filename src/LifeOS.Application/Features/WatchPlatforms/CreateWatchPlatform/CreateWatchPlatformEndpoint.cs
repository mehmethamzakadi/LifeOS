using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WatchPlatforms.CreateWatchPlatform;

public static class CreateWatchPlatformEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/watch-platforms", async (
            CreateWatchPlatformCommand command,
            CreateWatchPlatformHandler handler,
            IValidator<CreateWatchPlatformCommand> validator,
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
                    $"/api/watch-platforms/{response.Id}",
                    "İzleme platformu başarıyla oluşturuldu");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateWatchPlatformResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateWatchPlatform")
        .WithTags("WatchPlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.WatchPlatformsCreate)
        .Produces<ApiResult<CreateWatchPlatformResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateWatchPlatformResponse>>(StatusCodes.Status400BadRequest);
    }
}


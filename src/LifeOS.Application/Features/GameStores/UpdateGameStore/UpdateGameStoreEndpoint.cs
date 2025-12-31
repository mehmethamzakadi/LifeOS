using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GameStores.UpdateGameStore;

public static class UpdateGameStoreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/game-stores/{id:guid}", async (
            Guid id,
            UpdateGameStoreCommand command,
            UpdateGameStoreHandler handler,
            IValidator<UpdateGameStoreCommand> validator,
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
        .WithName("UpdateGameStore")
        .WithTags("GameStores")
        .RequireAuthorization(Domain.Constants.Permissions.GameStoresUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


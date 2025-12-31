using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GameStores.CreateGameStore;

public static class CreateGameStoreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/game-stores", async (
            CreateGameStoreCommand command,
            CreateGameStoreHandler handler,
            IValidator<CreateGameStoreCommand> validator,
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
                    $"/api/game-stores/{response.Id}",
                    "Oyun mağazası başarıyla oluşturuldu");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateGameStoreResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateGameStore")
        .WithTags("GameStores")
        .RequireAuthorization(Domain.Constants.Permissions.GameStoresCreate)
        .Produces<ApiResult<CreateGameStoreResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateGameStoreResponse>>(StatusCodes.Status400BadRequest);
    }
}


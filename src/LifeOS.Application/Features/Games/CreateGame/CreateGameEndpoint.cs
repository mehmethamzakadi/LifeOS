using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Games.CreateGame;

public static class CreateGameEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/games", async (
            CreateGameCommand command,
            CreateGameHandler handler,
            IValidator<CreateGameCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var response = await handler.HandleAsync(command, cancellationToken);
            return ApiResultExtensions.CreatedResult(
                response,
                $"/api/games/{response.Id}",
                ResponseMessages.Game.Created);
        })
        .WithName("CreateGame")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesCreate)
        .Produces<ApiResult<CreateGameResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateGameResponse>>(StatusCodes.Status400BadRequest);
    }
}


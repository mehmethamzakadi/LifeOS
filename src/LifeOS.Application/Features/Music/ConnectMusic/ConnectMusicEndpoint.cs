using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.ConnectMusic;

public static class ConnectMusicEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/music/connect", async (
            ConnectMusicCommand command,
            ConnectMusicHandler handler,
            IValidator<ConnectMusicCommand> validator,
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
        .WithName("ConnectMusic")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<ConnectMusicResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<ConnectMusicResponse>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<ConnectMusicResponse>>(StatusCodes.Status401Unauthorized);
    }
}


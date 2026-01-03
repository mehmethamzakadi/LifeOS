using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.GenerateMoodPlaylist;

public static class GenerateMoodPlaylistEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/music/generate-mood-playlist", async (
            GenerateMoodPlaylistCommand command,
            GenerateMoodPlaylistHandler handler,
            IValidator<GenerateMoodPlaylistCommand> validator,
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
        .WithName("GenerateMoodPlaylist")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<GenerateMoodPlaylistResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GenerateMoodPlaylistResponse>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<GenerateMoodPlaylistResponse>>(StatusCodes.Status401Unauthorized);
    }
}


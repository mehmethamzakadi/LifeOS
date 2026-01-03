using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.SaveTrack;

public static class SaveTrackEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/music/saved-tracks", async (
            SaveTrackCommand command,
            SaveTrackHandler handler,
            IValidator<SaveTrackCommand> validator,
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
        .WithName("SaveTrack")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<SaveTrackResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<SaveTrackResponse>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<SaveTrackResponse>>(StatusCodes.Status401Unauthorized);
    }
}


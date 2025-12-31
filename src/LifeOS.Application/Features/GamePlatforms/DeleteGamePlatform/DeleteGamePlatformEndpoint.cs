using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GamePlatforms.DeleteGamePlatform;

public static class DeleteGamePlatformEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/game-platforms/{id:guid}", async (
            Guid id,
            DeleteGamePlatformHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteGamePlatform")
        .WithTags("GamePlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.GamePlatformsDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


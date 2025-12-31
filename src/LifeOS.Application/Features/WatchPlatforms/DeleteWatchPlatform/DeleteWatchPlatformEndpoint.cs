using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WatchPlatforms.DeleteWatchPlatform;

public static class DeleteWatchPlatformEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/watch-platforms/{id:guid}", async (
            Guid id,
            DeleteWatchPlatformHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteWatchPlatform")
        .WithTags("WatchPlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.WatchPlatformsDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


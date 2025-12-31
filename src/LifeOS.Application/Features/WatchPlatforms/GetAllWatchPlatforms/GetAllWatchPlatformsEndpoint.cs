using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WatchPlatforms.GetAllWatchPlatforms;

public static class GetAllWatchPlatformsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/watch-platforms", async (
            GetAllWatchPlatformsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetAllWatchPlatforms")
        .WithTags("WatchPlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.WatchPlatformsViewAll)
        .Produces<ApiResult<List<GetAllWatchPlatformsResponse>>>(StatusCodes.Status200OK);
    }
}


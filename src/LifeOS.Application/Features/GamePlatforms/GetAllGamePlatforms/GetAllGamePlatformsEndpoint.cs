using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GamePlatforms.GetAllGamePlatforms;

public static class GetAllGamePlatformsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/game-platforms", async (
            GetAllGamePlatformsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetAllGamePlatforms")
        .WithTags("GamePlatforms")
        .RequireAuthorization(Domain.Constants.Permissions.GamePlatformsViewAll)
        .Produces<ApiResult<List<GetAllGamePlatformsResponse>>>(StatusCodes.Status200OK);
    }
}


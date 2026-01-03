using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.GetCurrentTrack;

public static class GetCurrentTrackEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/current-track", async (
            GetCurrentTrackHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetCurrentTrack")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<GetCurrentTrackResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetCurrentTrackResponse>>(StatusCodes.Status401Unauthorized);
    }
}


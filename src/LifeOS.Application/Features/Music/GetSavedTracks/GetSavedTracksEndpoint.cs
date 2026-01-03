using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.GetSavedTracks;

public static class GetSavedTracksEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/saved-tracks", async (
            GetSavedTracksHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetSavedTracks")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<GetSavedTracksResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetSavedTracksResponse>>(StatusCodes.Status401Unauthorized);
    }
}


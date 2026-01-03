using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.DeleteSavedTrack;

public static class DeleteSavedTrackEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/music/saved-tracks/{trackId:guid}", async (
            Guid trackId,
            DeleteSavedTrackHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(trackId, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteSavedTrack")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status401Unauthorized)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}


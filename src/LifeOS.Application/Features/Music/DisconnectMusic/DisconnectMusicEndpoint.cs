using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.DisconnectMusic;

public static class DisconnectMusicEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/music/disconnect", async (
            DisconnectMusicHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("DisconnectMusic")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<DisconnectMusicResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<DisconnectMusicResponse>>(StatusCodes.Status401Unauthorized);
    }
}


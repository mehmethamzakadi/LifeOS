using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.GetConnectionStatus;

public static class GetConnectionStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/connection-status", async (
            GetConnectionStatusHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetConnectionStatus")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<GetConnectionStatusResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetConnectionStatusResponse>>(StatusCodes.Status401Unauthorized);
    }
}


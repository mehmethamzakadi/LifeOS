using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.GetMusicAuthorizationUrl;

public static class GetMusicAuthorizationUrlEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/authorization-url", async (
            GetMusicAuthorizationUrlHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = handler.Handle();
            return result.ToResult();
        })
        .WithName("GetMusicAuthorizationUrl")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<GetMusicAuthorizationUrlResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetMusicAuthorizationUrlResponse>>(StatusCodes.Status401Unauthorized);
    }
}


using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.GetProfile;

public static class GetProfileEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/profile", async (
            GetProfileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetProfile")
        .WithTags("Profile")
        .Produces<ApiResult<GetProfileResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetProfileResponse>>(StatusCodes.Status401Unauthorized);
    }
}


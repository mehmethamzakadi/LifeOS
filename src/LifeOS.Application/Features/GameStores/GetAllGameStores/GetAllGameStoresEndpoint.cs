using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GameStores.GetAllGameStores;

public static class GetAllGameStoresEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/game-stores", async (
            GetAllGameStoresHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(cancellationToken);
            return result.ToResult();
        })
        .WithName("GetAllGameStores")
        .WithTags("GameStores")
        .RequireAuthorization(Domain.Constants.Permissions.GameStoresViewAll)
        .Produces<ApiResult<List<GetAllGameStoresResponse>>>(StatusCodes.Status200OK);
    }
}


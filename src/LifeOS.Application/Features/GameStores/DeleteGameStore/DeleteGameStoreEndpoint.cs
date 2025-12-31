using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.GameStores.DeleteGameStore;

public static class DeleteGameStoreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/game-stores/{id:guid}", async (
            Guid id,
            DeleteGameStoreHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteGameStore")
        .WithTags("GameStores")
        .RequireAuthorization(Domain.Constants.Permissions.GameStoresDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}


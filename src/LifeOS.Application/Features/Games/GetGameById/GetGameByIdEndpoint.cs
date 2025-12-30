using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Games.GetGameById;

public static class GetGameByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/games/{id}", async (
            Guid id,
            GetGameByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetGameById")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesRead)
        .Produces<ApiResult<GetGameByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetGameByIdResponse>>(StatusCodes.Status404NotFound);
    }
}


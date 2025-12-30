using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Games.SearchGames;

public static class SearchGamesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/games/search", async (
            DataGridRequest request,
            SearchGamesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchGames")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchGamesResponse>>>(StatusCodes.Status200OK);
    }
}


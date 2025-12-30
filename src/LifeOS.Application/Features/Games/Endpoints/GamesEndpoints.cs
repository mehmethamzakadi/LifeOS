using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Games.Endpoints;

public static class GamesEndpoints
{
    public static void MapGamesEndpoints(this IEndpointRouteBuilder app)
    {
        CreateGame.MapEndpoint(app);
        UpdateGame.MapEndpoint(app);
        DeleteGame.MapEndpoint(app);
        GetGameById.MapEndpoint(app);
        SearchGames.MapEndpoint(app);
    }
}


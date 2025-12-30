using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.Endpoints;

public static class DeleteGame
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/games/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var game = await context.Games
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (game is null)
                return Results.NotFound(new { Error = ResponseMessages.Game.NotFound });

            game.Delete();
            context.Games.Update(game);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.Game(game.Id));

            await cacheService.Add(
                CacheKeys.GameGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.NoContent();
        })
        .WithName("DeleteGame")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}


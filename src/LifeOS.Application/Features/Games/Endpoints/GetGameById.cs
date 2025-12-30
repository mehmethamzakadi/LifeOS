using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.Endpoints;

public static class GetGameById
{
    public sealed record Response(
        Guid Id,
        string Title,
        string? CoverUrl,
        GamePlatform Platform,
        GameStore Store,
        GameStatus Status,
        bool IsOwned);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/games/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.Game(id);
            var cacheValue = await cacheService.Get<Response>(cacheKey);
            if (cacheValue is not null)
                return Results.Ok(cacheValue);

            var game = await context.Games
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (game is null)
                return Results.NotFound(new { Error = "Oyun bilgisi bulunamadı." });

            var response = new Response(
                game.Id,
                game.Title,
                game.CoverUrl,
                game.Platform,
                game.Store,
                game.Status,
                game.IsOwned);

            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.Game),
                null);

            return Results.Ok(response);
        })
        .WithName("GetGameById")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Games.Endpoints;

public static class CreateGame
{
    public sealed record Request(
        string Title,
        string? CoverUrl = null,
        GamePlatform Platform = GamePlatform.PC,
        GameStore Store = GameStore.Steam,
        GameStatus Status = GameStatus.Backlog,
        bool IsOwned = false);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(g => g.Title)
                .NotEmpty().WithMessage("Oyun adı boş olmamalıdır!")
                .MinimumLength(2).WithMessage("Oyun adı en az 2 karakter olmalıdır!")
                .MaximumLength(200).WithMessage("Oyun adı en fazla 200 karakter olmalıdır!")
                .MustBePlainText("Oyun adı HTML veya script içeremez!");

            RuleFor(g => g.CoverUrl)
                .MaximumLength(500).WithMessage("Kapak URL'si en fazla 500 karakter olabilir!")
                .When(g => !string.IsNullOrWhiteSpace(g.CoverUrl));
        }
    }

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/games", async (
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var game = Game.Create(
                request.Title,
                request.CoverUrl,
                request.Platform,
                request.Store,
                request.Status,
                request.IsOwned);

            await context.Games.AddAsync(game, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Add(
                CacheKeys.Game(game.Id),
                new GetGameById.Response(
                    game.Id,
                    game.Title,
                    game.CoverUrl,
                    game.Platform,
                    game.Store,
                    game.Status,
                    game.IsOwned),
                DateTimeOffset.UtcNow.Add(CacheDurations.Game),
                null);

            await cache.Add(
                CacheKeys.GameGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.Created($"/api/games/{game.Id}", new Response(game.Id));
        })
        .WithName("CreateGame")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesCreate)
        .Produces<Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}


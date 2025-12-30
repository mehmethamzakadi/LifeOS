using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.Endpoints;

public static class SearchGames
{
    public sealed record Response : BaseEntityResponse
    {
        public string Title { get; init; } = string.Empty;
        public string? CoverUrl { get; init; }
        public GamePlatform Platform { get; init; }
        public GameStore Store { get; init; }
        public GameStatus Status { get; init; }
        public bool IsOwned { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/games/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var versionKey = CacheKeys.GameGridVersion();
            var versionToken = await cacheService.Get<string>(versionKey);
            if (string.IsNullOrWhiteSpace(versionToken))
            {
                versionToken = Guid.NewGuid().ToString("N");
                await cacheService.Add(versionKey, versionToken, null, null);
            }

            var cacheKey = CacheKeys.GameGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
            var cachedResponse = await cacheService.Get<PaginatedListResponse<Response>>(cacheKey);
            if (cachedResponse is not null)
            {
                return Results.Ok(cachedResponse);
            }

            var query = context.Games.AsNoTracking().AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var gamesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(gamesDynamic);
            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.GameGrid),
                null);

            return Results.Ok(response);
        })
        .WithName("SearchGames")
        .WithTags("Games")
        .RequireAuthorization(Domain.Constants.Permissions.GamesViewAll)
        .Produces<PaginatedListResponse<Response>>(StatusCodes.Status200OK);
    }
}


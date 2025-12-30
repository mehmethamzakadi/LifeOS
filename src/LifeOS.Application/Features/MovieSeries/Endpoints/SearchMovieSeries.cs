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

namespace LifeOS.Application.Features.MovieSeries.Endpoints;

public static class SearchMovieSeries
{
    public sealed record Response : BaseEntityResponse
    {
        public string Title { get; init; } = string.Empty;
        public string? CoverUrl { get; init; }
        public MovieSeriesType Type { get; init; }
        public MovieSeriesPlatform Platform { get; init; }
        public int? CurrentSeason { get; init; }
        public int? CurrentEpisode { get; init; }
        public MovieSeriesStatus Status { get; init; }
        public int? Rating { get; init; }
        public string? PersonalNote { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/movieseries/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var versionKey = CacheKeys.MovieSeriesGridVersion();
            var versionToken = await cacheService.Get<string>(versionKey);
            if (string.IsNullOrWhiteSpace(versionToken))
            {
                versionToken = Guid.NewGuid().ToString("N");
                await cacheService.Add(versionKey, versionToken, null, null);
            }

            var cacheKey = CacheKeys.MovieSeriesGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
            var cachedResponse = await cacheService.Get<PaginatedListResponse<Response>>(cacheKey);
            if (cachedResponse is not null)
            {
                return Results.Ok(cachedResponse);
            }

            var query = context.MovieSeries.AsNoTracking().AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var movieSeriesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(movieSeriesDynamic);
            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeriesGrid),
                null);

            return Results.Ok(response);
        })
        .WithName("SearchMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesViewAll)
        .Produces<PaginatedListResponse<Response>>(StatusCodes.Status200OK);
    }
}


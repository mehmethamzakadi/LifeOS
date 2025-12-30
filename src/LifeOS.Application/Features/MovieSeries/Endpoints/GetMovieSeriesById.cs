using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.Endpoints;

public static class GetMovieSeriesById
{
    public sealed record Response(
        Guid Id,
        string Title,
        string? CoverUrl,
        MovieSeriesType Type,
        MovieSeriesPlatform Platform,
        int? CurrentSeason,
        int? CurrentEpisode,
        MovieSeriesStatus Status,
        int? Rating,
        string? PersonalNote);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/movieseries/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.MovieSeries(id);
            var cacheValue = await cacheService.Get<Response>(cacheKey);
            if (cacheValue is not null)
                return ApiResultExtensions.Success(cacheValue, "Film/Dizi bilgisi başarıyla getirildi").ToResult();

            var movieSeries = await context.MovieSeries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (movieSeries is null)
                return ApiResultExtensions.Failure<Response>("Film/Dizi bilgisi bulunamadı.").ToResult();

            var response = new Response(
                movieSeries.Id,
                movieSeries.Title,
                movieSeries.CoverUrl,
                movieSeries.Type,
                movieSeries.Platform,
                movieSeries.CurrentSeason,
                movieSeries.CurrentEpisode,
                movieSeries.Status,
                movieSeries.Rating,
                movieSeries.PersonalNote);

            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
                null);

            return ApiResultExtensions.Success(response, "Film/Dizi bilgisi başarıyla getirildi").ToResult();
        })
        .WithName("GetMovieSeriesById")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesRead)
        .Produces<ApiResult<Response>>(StatusCodes.Status200OK)
        .Produces<ApiResult<Response>>(StatusCodes.Status404NotFound);
    }
}


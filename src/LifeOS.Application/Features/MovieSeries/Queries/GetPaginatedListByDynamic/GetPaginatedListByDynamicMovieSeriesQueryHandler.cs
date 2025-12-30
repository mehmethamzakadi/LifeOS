using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicMovieSeriesQueryHandler(
    LifeOSDbContext context,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicMovieSeriesQuery, PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>> Handle(GetPaginatedListByDynamicMovieSeriesQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.MovieSeriesGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.MovieSeriesGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var query = context.MovieSeries.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DataGridRequest.DynamicQuery);
        var movieSeriesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>>(movieSeriesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeriesGrid),
            null);

        return response;
    }
}


using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using MediatR;
using MovieSeriesEntity = LifeOS.Domain.Entities.MovieSeries;

namespace LifeOS.Application.Features.MovieSeries.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicMovieSeriesQueryHandler(
    IMovieSeriesRepository movieSeriesRepository,
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

        Paginate<MovieSeriesEntity> movieSeriesDynamic = await movieSeriesRepository.GetPaginatedListByDynamicAsync(
            dynamic: request.DataGridRequest.DynamicQuery,
            index: pagination.PageIndex,
            size: pagination.PageSize,
            include: null,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicMovieSeriesResponse>>(movieSeriesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeriesGrid),
            null);

        return response;
    }
}


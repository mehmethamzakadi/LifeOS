using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicGamesQueryHandler(
    LifeOSDbContext context,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicGamesQuery, PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>> Handle(GetPaginatedListByDynamicGamesQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.GameGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.GameGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var query = context.Games.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DataGridRequest.DynamicQuery);
        var gamesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<GetPaginatedListByDynamicGamesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicGamesResponse>>(gamesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.GameGrid),
            null);

        return response;
    }
}


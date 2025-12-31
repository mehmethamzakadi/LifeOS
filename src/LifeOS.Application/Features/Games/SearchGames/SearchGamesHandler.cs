using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Games.SearchGames;

public sealed class SearchGamesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchGamesHandler(
        LifeOSDbContext context,
        IMapper mapper,
        ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchGamesResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var versionKey = CacheKeys.GameGridVersion();
        var versionToken = await _cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await _cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.GameGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
        var cachedResponse = await _cacheService.Get<PaginatedListResponse<SearchGamesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return ApiResultExtensions.Success(cachedResponse, "Oyunlar başarıyla getirildi");
        }

        var query = _context.Games
            .Include(g => g.GamePlatform)
            .Include(g => g.GameStore)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var gamesDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        // Manual mapping because AutoMapper can't handle navigation properties easily
        var responseItems = gamesDynamic.Items.Select(g => new SearchGamesResponse
        {
            Id = g.Id,
            Title = g.Title,
            CoverUrl = g.CoverUrl,
            GamePlatformId = g.GamePlatformId,
            GamePlatformName = g.GamePlatform.Name,
            GameStoreId = g.GameStoreId,
            GameStoreName = g.GameStore.Name,
            Status = g.Status,
            IsOwned = g.IsOwned,
            CreatedDate = g.CreatedDate
        }).ToList();

        var response = new PaginatedListResponse<SearchGamesResponse>
        {
            Items = responseItems,
            Index = gamesDynamic.Index,
            Size = gamesDynamic.Size,
            Count = gamesDynamic.Count,
            Pages = gamesDynamic.Pages
        };
        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.GameGrid),
            null);

        return ApiResultExtensions.Success(response, "Oyunlar başarıyla getirildi");
    }
}


using AutoMapper;
using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.SearchWalletTransactions;

public sealed class SearchWalletTransactionsHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public SearchWalletTransactionsHandler(
        LifeOSDbContext context,
        IMapper mapper,
        ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchWalletTransactionsResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var versionKey = CacheKeys.WalletTransactionGridVersion();
        var versionToken = await _cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await _cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.WalletTransactionGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
        var cachedResponse = await _cacheService.Get<PaginatedListResponse<SearchWalletTransactionsResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return ApiResultExtensions.Success(cachedResponse, "Cüzdan işlemleri başarıyla getirildi");
        }

        var query = _context.WalletTransactions.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var walletTransactionsDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<SearchWalletTransactionsResponse> response = _mapper.Map<PaginatedListResponse<SearchWalletTransactionsResponse>>(walletTransactionsDynamic);
        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransactionGrid),
            null);

        return ApiResultExtensions.Success(response, "Cüzdan işlemleri başarıyla getirildi");
    }
}


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

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicWalletTransactionsQueryHandler(
    LifeOSDbContext context,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicWalletTransactionsQuery, PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>> Handle(GetPaginatedListByDynamicWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.WalletTransactionGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.WalletTransactionGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        var query = context.WalletTransactions.AsNoTracking().AsQueryable();
        query = query.ToDynamic(request.DataGridRequest.DynamicQuery);
        var walletTransactionsDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

        PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>>(walletTransactionsDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransactionGrid),
            null);

        return response;
    }
}


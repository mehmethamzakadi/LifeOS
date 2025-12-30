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

namespace LifeOS.Application.Features.WalletTransactions.Endpoints;

public static class SearchWalletTransactions
{
    public sealed record Response : BaseEntityResponse
    {
        public string Title { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public TransactionType Type { get; init; }
        public TransactionCategory Category { get; init; }
        public DateTime TransactionDate { get; init; }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/wallettransactions/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var versionKey = CacheKeys.WalletTransactionGridVersion();
            var versionToken = await cacheService.Get<string>(versionKey);
            if (string.IsNullOrWhiteSpace(versionToken))
            {
                versionToken = Guid.NewGuid().ToString("N");
                await cacheService.Add(versionKey, versionToken, null, null);
            }

            var cacheKey = CacheKeys.WalletTransactionGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DynamicQuery);
            var cachedResponse = await cacheService.Get<PaginatedListResponse<Response>>(cacheKey);
            if (cachedResponse is not null)
            {
                return ApiResultExtensions.Success(cachedResponse, "Cüzdan işlemleri başarıyla getirildi").ToResult();
            }

            var query = context.WalletTransactions.AsNoTracking().AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var walletTransactionsDynamic = await query.ToPaginateAsync(pagination.PageIndex, pagination.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(walletTransactionsDynamic);
            await cacheService.Add(
                cacheKey,
                response,
                DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransactionGrid),
                null);

            return ApiResultExtensions.Success(response, "Cüzdan işlemleri başarıyla getirildi").ToResult();
        })
        .WithName("SearchWalletTransactions")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsViewAll)
        .Produces<ApiResult<PaginatedListResponse<Response>>>(StatusCodes.Status200OK);
    }
}


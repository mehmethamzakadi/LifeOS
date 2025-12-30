using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;

public sealed class GetWalletTransactionByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public GetWalletTransactionByIdHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<GetWalletTransactionByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.WalletTransaction(id);
        var cacheValue = await _cacheService.Get<GetWalletTransactionByIdResponse>(cacheKey);
        if (cacheValue is not null)
            return ApiResultExtensions.Success(cacheValue, "Cüzdan işlemi başarıyla getirildi");

        var walletTransaction = await _context.WalletTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (walletTransaction is null)
            return ApiResultExtensions.Failure<GetWalletTransactionByIdResponse>("Cüzdan işlemi bulunamadı.");

        var response = new GetWalletTransactionByIdResponse(
            walletTransaction.Id,
            walletTransaction.Title,
            walletTransaction.Amount,
            walletTransaction.Type,
            walletTransaction.Category,
            walletTransaction.TransactionDate);

        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransaction),
            null);

        return ApiResultExtensions.Success(response, "Cüzdan işlemi başarıyla getirildi");
    }
}


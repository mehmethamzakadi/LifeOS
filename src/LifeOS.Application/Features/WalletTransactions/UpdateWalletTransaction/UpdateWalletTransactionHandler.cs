using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.UpdateWalletTransaction;

public sealed class UpdateWalletTransactionHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateWalletTransactionHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateWalletTransactionCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var walletTransaction = await _context.WalletTransactions
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);

        if (walletTransaction is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.WalletTransaction.NotFound);
        }

        walletTransaction.Update(
            command.Title,
            command.Amount,
            command.Type,
            command.Category,
            command.TransactionDate);

        _context.WalletTransactions.Update(walletTransaction);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.WalletTransaction(walletTransaction.Id),
            new GetWalletTransactionByIdResponse(
                walletTransaction.Id,
                walletTransaction.Title,
                walletTransaction.Amount,
                walletTransaction.Type,
                walletTransaction.Category,
                walletTransaction.TransactionDate),
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransaction),
            null);

        await _cache.Add(
            CacheKeys.WalletTransactionGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.WalletTransaction.Updated);
    }
}


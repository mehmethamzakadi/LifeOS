using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.DeleteWalletTransaction;

public sealed class DeleteWalletTransactionHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteWalletTransactionHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var walletTransaction = await _context.WalletTransactions
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        
        if (walletTransaction is null)
            return ApiResultExtensions.Failure(ResponseMessages.WalletTransaction.NotFound);

        walletTransaction.Delete();
        _context.WalletTransactions.Update(walletTransaction);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Remove(CacheKeys.WalletTransaction(walletTransaction.Id));

        await _cacheService.Add(
            CacheKeys.WalletTransactionGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.WalletTransaction.Deleted);
    }
}


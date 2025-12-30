using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;

namespace LifeOS.Application.Features.WalletTransactions.CreateWalletTransaction;

public sealed class CreateWalletTransactionHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateWalletTransactionHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateWalletTransactionResponse> HandleAsync(
        CreateWalletTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var walletTransaction = WalletTransaction.Create(
            command.Title,
            command.Amount,
            command.Type,
            command.Category,
            command.TransactionDate);

        await _context.WalletTransactions.AddAsync(walletTransaction, cancellationToken);
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

        return new CreateWalletTransactionResponse(walletTransaction.Id);
    }
}


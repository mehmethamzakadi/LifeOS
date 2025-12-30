using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.WalletTransactions.UpdateWalletTransaction;

public sealed record UpdateWalletTransactionCommand(
    Guid Id,
    string Title,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    DateTime TransactionDate);


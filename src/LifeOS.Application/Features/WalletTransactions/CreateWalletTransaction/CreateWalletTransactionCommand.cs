using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.WalletTransactions.CreateWalletTransaction;

public sealed record CreateWalletTransactionCommand(
    string Title,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    DateTime TransactionDate);


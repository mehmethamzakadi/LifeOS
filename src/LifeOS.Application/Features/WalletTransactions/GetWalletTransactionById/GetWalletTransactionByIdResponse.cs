using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;

public sealed record GetWalletTransactionByIdResponse(
    Guid Id,
    string Title,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    DateTime TransactionDate);


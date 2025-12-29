using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetById;

public sealed record GetByIdWalletTransactionResponse(
    Guid Id,
    string Title,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    DateTime TransactionDate);


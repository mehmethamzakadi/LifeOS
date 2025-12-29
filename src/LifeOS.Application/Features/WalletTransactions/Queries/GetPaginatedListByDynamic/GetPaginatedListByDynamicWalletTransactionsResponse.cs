using LifeOS.Application.Common;
using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicWalletTransactionsResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public TransactionCategory Category { get; init; }
    public DateTime TransactionDate { get; init; }
}


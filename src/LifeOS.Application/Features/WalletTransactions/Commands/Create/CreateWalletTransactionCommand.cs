using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Enums;
using MediatR;

namespace LifeOS.Application.Features.WalletTransactions.Commands.Create;

public sealed record CreateWalletTransactionCommand(
    string Title,
    decimal Amount,
    TransactionType Type,
    TransactionCategory Category,
    DateTime TransactionDate) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.WalletTransactionGridVersion();
    }
}


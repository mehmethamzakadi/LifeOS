using LifeOS.Application.Behaviors;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.WalletTransactions.Commands.Delete;

public sealed record DeleteWalletTransactionCommand(Guid Id) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.WalletTransaction(Id);
        yield return CacheKeys.WalletTransactionGridVersion();
    }
}


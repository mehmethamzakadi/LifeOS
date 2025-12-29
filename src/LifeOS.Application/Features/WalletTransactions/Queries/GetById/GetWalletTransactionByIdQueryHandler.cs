using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetById;

public sealed class GetWalletTransactionByIdQueryHandler(
    IWalletTransactionRepository walletTransactionRepository,
    ICacheService cacheService) : IRequestHandler<GetByIdWalletTransactionQuery, IDataResult<GetByIdWalletTransactionResponse>>
{
    public async Task<IDataResult<GetByIdWalletTransactionResponse>> Handle(GetByIdWalletTransactionQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.WalletTransaction(request.Id);
        var cacheValue = await cacheService.Get<GetByIdWalletTransactionResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdWalletTransactionResponse>(cacheValue);

        var walletTransaction = await walletTransactionRepository.GetByIdAsync(request.Id, cancellationToken);

        if (walletTransaction is null)
            return new ErrorDataResult<GetByIdWalletTransactionResponse>("Cüzdan işlemi bulunamadı.");

        var response = new GetByIdWalletTransactionResponse(
            walletTransaction.Id,
            walletTransaction.Title,
            walletTransaction.Amount,
            walletTransaction.Type,
            walletTransaction.Category,
            walletTransaction.TransactionDate);

        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransaction),
            null);

        return new SuccessDataResult<GetByIdWalletTransactionResponse>(response);
    }
}


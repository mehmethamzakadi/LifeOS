using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Repositories;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.WalletTransactions.Commands.Delete;

public sealed class DeleteWalletTransactionCommandHandler(
    IWalletTransactionRepository walletTransactionRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteWalletTransactionCommand, IResult>
{
    public async Task<IResult> Handle(DeleteWalletTransactionCommand request, CancellationToken cancellationToken)
    {
        var walletTransaction = await walletTransactionRepository.GetAsync(predicate: x => x.Id == request.Id, enableTracking: true, cancellationToken: cancellationToken);
        if (walletTransaction is null)
            return new ErrorResult(ResponseMessages.WalletTransaction.NotFound);

        walletTransaction.Delete();
        walletTransactionRepository.Update(walletTransaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.WalletTransaction(walletTransaction.Id));

        await cacheService.Add(
            CacheKeys.WalletTransactionGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.WalletTransaction.Deleted);
    }
}


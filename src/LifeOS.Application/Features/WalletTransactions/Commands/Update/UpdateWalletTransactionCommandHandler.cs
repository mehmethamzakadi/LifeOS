using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.WalletTransactions.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.WalletTransactions.Commands.Update;

public sealed class UpdateWalletTransactionCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateWalletTransactionCommand, IResult>
{
    public async Task<IResult> Handle(UpdateWalletTransactionCommand request, CancellationToken cancellationToken)
    {
        var walletTransaction = await context.WalletTransactions
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (walletTransaction is null)
        {
            return new ErrorResult(ResponseMessages.WalletTransaction.NotFound);
        }

        walletTransaction.Update(
            request.Title,
            request.Amount,
            request.Type,
            request.Category,
            request.TransactionDate);

        context.WalletTransactions.Update(walletTransaction);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Add(
            CacheKeys.WalletTransaction(walletTransaction.Id),
            new GetByIdWalletTransactionResponse(
                walletTransaction.Id,
                walletTransaction.Title,
                walletTransaction.Amount,
                walletTransaction.Type,
                walletTransaction.Category,
                walletTransaction.TransactionDate),
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransaction),
            null);

        await cacheService.Add(
            CacheKeys.WalletTransactionGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.WalletTransaction.Updated);
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.WalletTransactions.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.WalletTransactions.Commands.Create;

public sealed class CreateWalletTransactionCommandHandler(
    LifeOSDbContext context,
    ICacheService cache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateWalletTransactionCommand, IResult>
{
    public async Task<IResult> Handle(CreateWalletTransactionCommand request, CancellationToken cancellationToken)
    {
        var walletTransaction = WalletTransaction.Create(
            request.Title,
            request.Amount,
            request.Type,
            request.Category,
            request.TransactionDate);

        await context.WalletTransactions.AddAsync(walletTransaction, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.Add(
            CacheKeys.WalletTransaction(walletTransaction.Id),
            new GetByIdWalletTransactionResponse(
                Id: walletTransaction.Id,
                Title: walletTransaction.Title,
                Amount: walletTransaction.Amount,
                Type: walletTransaction.Type,
                Category: walletTransaction.Category,
                TransactionDate: walletTransaction.TransactionDate),
            DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransaction),
            null);

        await cache.Add(
            CacheKeys.WalletTransactionGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.WalletTransaction.Created);
    }
}


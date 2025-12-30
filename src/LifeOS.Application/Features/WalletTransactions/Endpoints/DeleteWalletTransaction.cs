using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.Endpoints;

public static class DeleteWalletTransaction
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/wallettransactions/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var walletTransaction = await context.WalletTransactions
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
            if (walletTransaction is null)
                return Results.NotFound(new { Error = ResponseMessages.WalletTransaction.NotFound });

            walletTransaction.Delete();
            context.WalletTransactions.Update(walletTransaction);
            await context.SaveChangesAsync(cancellationToken);

            await cacheService.Remove(CacheKeys.WalletTransaction(walletTransaction.Id));

            await cacheService.Add(
                CacheKeys.WalletTransactionGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.NoContent();
        })
        .WithName("DeleteWalletTransaction")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsDelete)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}


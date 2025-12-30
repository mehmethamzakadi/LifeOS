using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.Endpoints;

public static class GetWalletTransactionById
{
    public sealed record Response(
        Guid Id,
        string Title,
        decimal Amount,
        TransactionType Type,
        TransactionCategory Category,
        DateTime TransactionDate);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/wallettransactions/{id}", async (
            Guid id,
            LifeOSDbContext context,
            ICacheService cacheService,
            CancellationToken cancellationToken) =>
        {
            var cacheKey = CacheKeys.WalletTransaction(id);
            var cacheValue = await cacheService.Get<Response>(cacheKey);
            if (cacheValue is not null)
                return Results.Ok(cacheValue);

            var walletTransaction = await context.WalletTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (walletTransaction is null)
                return Results.NotFound(new { Error = "Cüzdan işlemi bulunamadı." });

            var response = new Response(
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

            return Results.Ok(response);
        })
        .WithName("GetWalletTransactionById")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsRead)
        .Produces<Response>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}


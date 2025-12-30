using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WalletTransactions.DeleteWalletTransaction;

public static class DeleteWalletTransactionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/wallettransactions/{id}", async (
            Guid id,
            DeleteWalletTransactionHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("DeleteWalletTransaction")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsDelete)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}


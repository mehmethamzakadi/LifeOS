using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;

public static class GetWalletTransactionByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/wallettransactions/{id}", async (
            Guid id,
            GetWalletTransactionByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetWalletTransactionById")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsRead)
        .Produces<ApiResult<GetWalletTransactionByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetWalletTransactionByIdResponse>>(StatusCodes.Status404NotFound);
    }
}


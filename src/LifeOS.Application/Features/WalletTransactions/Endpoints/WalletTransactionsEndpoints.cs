using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WalletTransactions.Endpoints;

public static class WalletTransactionsEndpoints
{
    public static void MapWalletTransactionsEndpoints(this IEndpointRouteBuilder app)
    {
        CreateWalletTransaction.MapEndpoint(app);
        UpdateWalletTransaction.MapEndpoint(app);
        DeleteWalletTransaction.MapEndpoint(app);
        GetWalletTransactionById.MapEndpoint(app);
        SearchWalletTransactions.MapEndpoint(app);
    }
}


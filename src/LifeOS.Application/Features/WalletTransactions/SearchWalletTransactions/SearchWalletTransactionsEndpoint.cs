using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WalletTransactions.SearchWalletTransactions;

public static class SearchWalletTransactionsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/wallettransactions/search", async (
            DataGridRequest request,
            SearchWalletTransactionsHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchWalletTransactions")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchWalletTransactionsResponse>>>(StatusCodes.Status200OK);
    }
}


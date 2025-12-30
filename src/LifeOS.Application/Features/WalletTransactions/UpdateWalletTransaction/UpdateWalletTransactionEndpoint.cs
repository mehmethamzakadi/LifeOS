using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WalletTransactions.UpdateWalletTransaction;

public static class UpdateWalletTransactionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/wallettransactions/{id}", async (
            Guid id,
            UpdateWalletTransactionCommand command,
            UpdateWalletTransactionHandler handler,
            IValidator<UpdateWalletTransactionCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(id, command, cancellationToken);
            return result.ToResult();
        })
        .WithName("UpdateWalletTransaction")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}


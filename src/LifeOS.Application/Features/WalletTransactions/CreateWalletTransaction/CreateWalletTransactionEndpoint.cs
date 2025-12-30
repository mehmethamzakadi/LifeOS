using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.WalletTransactions.CreateWalletTransaction;

public static class CreateWalletTransactionEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/wallettransactions", async (
            CreateWalletTransactionCommand command,
            CreateWalletTransactionHandler handler,
            IValidator<CreateWalletTransactionCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var response = await handler.HandleAsync(command, cancellationToken);
            return ApiResultExtensions.CreatedResult(
                response,
                $"/api/wallettransactions/{response.Id}",
                ResponseMessages.WalletTransaction.Created);
        })
        .WithName("CreateWalletTransaction")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsCreate)
        .Produces<ApiResult<CreateWalletTransactionResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateWalletTransactionResponse>>(StatusCodes.Status400BadRequest);
    }
}


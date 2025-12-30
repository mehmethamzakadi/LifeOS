using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.WalletTransactions.Endpoints;

public static class UpdateWalletTransaction
{
    public sealed record Request(
        Guid Id,
        string Title,
        decimal Amount,
        TransactionType Type,
        TransactionCategory Category,
        DateTime TransactionDate);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(w => w.Id)
                .NotEmpty().WithMessage("İşlem ID'si boş olamaz!");

            RuleFor(w => w.Title)
                .NotEmpty().WithMessage("İşlem başlığı boş olmamalıdır!")
                .MinimumLength(2).WithMessage("İşlem başlığı en az 2 karakter olmalıdır!")
                .MaximumLength(200).WithMessage("İşlem başlığı en fazla 200 karakter olmalıdır!")
                .MustBePlainText("İşlem başlığı HTML veya script içeremez!");

            RuleFor(w => w.Amount)
                .NotEqual(0).WithMessage("İşlem tutarı 0 olamaz!");

            RuleFor(w => w.Amount)
                .GreaterThan(0).WithMessage("Gelir işlemleri için tutar 0'dan büyük olmalıdır!")
                .When(w => w.Type == TransactionType.Income);

            RuleFor(w => w.Amount)
                .LessThan(0).WithMessage("Gider işlemleri için tutar negatif olmalıdır!")
                .When(w => w.Type == TransactionType.Expense);
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/wallettransactions/{id}", async (
            Guid id,
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id)
                return ApiResultExtensions.Failure("ID uyuşmazlığı").ToResult();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var walletTransaction = await context.WalletTransactions
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

            if (walletTransaction is null)
            {
                return ApiResultExtensions.Failure(ResponseMessages.WalletTransaction.NotFound).ToResult();
            }

            walletTransaction.Update(
                request.Title,
                request.Amount,
                request.Type,
                request.Category,
                request.TransactionDate);

            context.WalletTransactions.Update(walletTransaction);
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Add(
                CacheKeys.WalletTransaction(walletTransaction.Id),
                new GetWalletTransactionById.Response(
                    walletTransaction.Id,
                    walletTransaction.Title,
                    walletTransaction.Amount,
                    walletTransaction.Type,
                    walletTransaction.Category,
                    walletTransaction.TransactionDate),
                DateTimeOffset.UtcNow.Add(CacheDurations.WalletTransaction),
                null);

            await cache.Add(
                CacheKeys.WalletTransactionGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return ApiResultExtensions.Success(ResponseMessages.WalletTransaction.Updated).ToResult();
        })
        .WithName("UpdateWalletTransaction")
        .WithTags("WalletTransactions")
        .RequireAuthorization(Domain.Constants.Permissions.WalletTransactionsUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}


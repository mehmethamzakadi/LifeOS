using LifeOS.Application.Common.Security;
using LifeOS.Domain.Enums;
using FluentValidation;

namespace LifeOS.Application.Features.WalletTransactions.Commands.Create;

public sealed class CreateWalletTransactionValidator : AbstractValidator<CreateWalletTransactionCommand>
{
    public CreateWalletTransactionValidator()
    {
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


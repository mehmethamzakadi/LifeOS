using FluentValidation;
using LifeOS.Application.Common.Security;

namespace LifeOS.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Kategori adı bilgisi boş olmamalıdır!")
            .MinimumLength(5).WithMessage("Kategori adı en az 5 karakter olmalıdır!")
            .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olmalıdır!")
            .MustBePlainText("Kategori adı HTML veya script içeremez!");

        RuleFor(c => c.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir!")
            .When(c => !string.IsNullOrWhiteSpace(c.Description));
    }
}


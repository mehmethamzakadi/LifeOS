using FluentValidation;
using LifeOS.Application.Common.Security;

namespace LifeOS.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Kategori ID'si boş olamaz!");

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


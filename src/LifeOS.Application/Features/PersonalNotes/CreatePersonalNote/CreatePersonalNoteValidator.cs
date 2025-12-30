using FluentValidation;
using LifeOS.Application.Common.Security;

namespace LifeOS.Application.Features.PersonalNotes.CreatePersonalNote;

public sealed class CreatePersonalNoteValidator : AbstractValidator<CreatePersonalNoteCommand>
{
    public CreatePersonalNoteValidator()
    {
        RuleFor(p => p.Title)
            .NotEmpty().WithMessage("Not başlığı boş olmamalıdır!")
            .MinimumLength(2).WithMessage("Not başlığı en az 2 karakter olmalıdır!")
            .MaximumLength(200).WithMessage("Not başlığı en fazla 200 karakter olmalıdır!")
            .MustBePlainText("Not başlığı HTML veya script içeremez!");

        RuleFor(p => p.Content)
            .NotEmpty().WithMessage("Not içeriği boş olmamalıdır!");

        RuleFor(p => p.Category)
            .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir!")
            .When(p => !string.IsNullOrWhiteSpace(p.Category));

        RuleFor(p => p.Tags)
            .MaximumLength(500).WithMessage("Etiketler en fazla 500 karakter olabilir!")
            .When(p => !string.IsNullOrWhiteSpace(p.Tags));
    }
}


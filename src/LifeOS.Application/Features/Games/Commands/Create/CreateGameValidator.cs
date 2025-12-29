using LifeOS.Application.Common.Security;
using FluentValidation;

namespace LifeOS.Application.Features.Games.Commands.Create;

public sealed class CreateGameValidator : AbstractValidator<CreateGameCommand>
{
    public CreateGameValidator()
    {
        RuleFor(g => g.Title)
            .NotEmpty().WithMessage("Oyun adı boş olmamalıdır!")
            .MinimumLength(2).WithMessage("Oyun adı en az 2 karakter olmalıdır!")
            .MaximumLength(200).WithMessage("Oyun adı en fazla 200 karakter olmalıdır!")
            .MustBePlainText("Oyun adı HTML veya script içeremez!");

        RuleFor(g => g.CoverUrl)
            .MaximumLength(500).WithMessage("Kapak URL'si en fazla 500 karakter olabilir!")
            .When(g => !string.IsNullOrWhiteSpace(g.CoverUrl));
    }
}


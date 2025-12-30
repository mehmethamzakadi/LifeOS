using FluentValidation;
using LifeOS.Application.Common.Security;

namespace LifeOS.Application.Features.Games.UpdateGame;

public sealed class UpdateGameValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameValidator()
    {
        RuleFor(g => g.Id)
            .NotEmpty().WithMessage("Oyun ID'si boş olamaz!");

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


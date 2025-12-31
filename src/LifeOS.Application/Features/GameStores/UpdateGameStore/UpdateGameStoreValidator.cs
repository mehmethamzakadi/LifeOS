using FluentValidation;

namespace LifeOS.Application.Features.GameStores.UpdateGameStore;

public sealed class UpdateGameStoreValidator : AbstractValidator<UpdateGameStoreCommand>
{
    public UpdateGameStoreValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Mağaza ID boş olamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Mağaza adı boş olamaz")
            .MaximumLength(100).WithMessage("Mağaza adı en fazla 100 karakter olabilir");
    }
}


using FluentValidation;

namespace LifeOS.Application.Features.GameStores.CreateGameStore;

public sealed class CreateGameStoreValidator : AbstractValidator<CreateGameStoreCommand>
{
    public CreateGameStoreValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Mağaza adı boş olamaz")
            .MaximumLength(100).WithMessage("Mağaza adı en fazla 100 karakter olabilir");
    }
}


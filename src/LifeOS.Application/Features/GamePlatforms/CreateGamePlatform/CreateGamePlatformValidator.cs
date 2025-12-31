using FluentValidation;

namespace LifeOS.Application.Features.GamePlatforms.CreateGamePlatform;

public sealed class CreateGamePlatformValidator : AbstractValidator<CreateGamePlatformCommand>
{
    public CreateGamePlatformValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Platform adı boş olamaz")
            .MaximumLength(100).WithMessage("Platform adı en fazla 100 karakter olabilir");
    }
}


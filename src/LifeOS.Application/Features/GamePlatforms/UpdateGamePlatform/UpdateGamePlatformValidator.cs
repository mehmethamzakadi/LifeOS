using FluentValidation;

namespace LifeOS.Application.Features.GamePlatforms.UpdateGamePlatform;

public sealed class UpdateGamePlatformValidator : AbstractValidator<UpdateGamePlatformCommand>
{
    public UpdateGamePlatformValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Platform ID boş olamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Platform adı boş olamaz")
            .MaximumLength(100).WithMessage("Platform adı en fazla 100 karakter olabilir");
    }
}


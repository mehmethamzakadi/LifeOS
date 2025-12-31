using FluentValidation;

namespace LifeOS.Application.Features.WatchPlatforms.CreateWatchPlatform;

public sealed class CreateWatchPlatformValidator : AbstractValidator<CreateWatchPlatformCommand>
{
    public CreateWatchPlatformValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Platform adı boş olamaz")
            .MaximumLength(100).WithMessage("Platform adı en fazla 100 karakter olabilir");
    }
}


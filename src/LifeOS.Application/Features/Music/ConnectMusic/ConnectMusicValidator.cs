using FluentValidation;

namespace LifeOS.Application.Features.Music.ConnectMusic;

public sealed class ConnectMusicValidator : AbstractValidator<ConnectMusicCommand>
{
    public ConnectMusicValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("Authorization code boş olamaz!");

        RuleFor(c => c.State)
            .NotEmpty().WithMessage("State parametresi boş olamaz!");
    }
}


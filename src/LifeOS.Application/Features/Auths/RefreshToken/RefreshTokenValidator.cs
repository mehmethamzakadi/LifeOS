using FluentValidation;

namespace LifeOS.Application.Features.Auths.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token gereklidir.")
            .MinimumLength(20).WithMessage("Geçersiz refresh token formatı.");
    }
}

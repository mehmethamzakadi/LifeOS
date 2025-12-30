using FluentValidation;

namespace LifeOS.Application.Features.Auths.PasswordReset;

public sealed class PasswordResetValidator : AbstractValidator<PasswordResetCommand>
{
    public PasswordResetValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi gereklidir")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi girin!");
    }
}


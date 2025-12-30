using FluentValidation;

namespace LifeOS.Application.Features.Auths.PasswordVerify;

public sealed class PasswordVerifyValidator : AbstractValidator<PasswordVerifyCommand>
{
    public PasswordVerifyValidator()
    {
        RuleFor(x => x.ResetToken)
            .NotEmpty().WithMessage("Reset token gereklidir");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si gereklidir");
    }
}


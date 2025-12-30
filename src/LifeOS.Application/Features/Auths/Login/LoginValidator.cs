using FluentValidation;
using FluentValidation.Validators;

namespace LifeOS.Application.Features.Auths.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(u => u.Email).EmailAddress(EmailValidationMode.AspNetCoreCompatible).WithMessage("Email adresi geçersiz!");
    }
}


using FluentValidation;

namespace LifeOS.Application.Features.Users.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre gereklidir");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre gereklidir")
            .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Yeni şifre en fazla 100 karakter olabilir")
            .Matches("[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir!")
            .Matches("[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir!")
            .Matches("[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir!")
            .Matches("[^a-zA-Z0-9]").WithMessage("Yeni şifre en az bir özel karakter içermelidir!");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Şifre onayı gereklidir")
            .Equal(x => x.NewPassword).WithMessage("Yeni şifre ve şifre onayı eşleşmiyor");
    }
}


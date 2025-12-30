using FluentValidation;
using System.Text.RegularExpressions;

namespace LifeOS.Application.Features.Users.CreateUser;

public sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UserNameRegex = new(
        @"^[a-zA-Z0-9_-]{3,50}$",
        RegexOptions.Compiled);

    public CreateUserValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Kullanıcı adı gereklidir")
            .Length(3, 50).WithMessage("Kullanıcı adı 3 ile 50 karakter arasında olmalıdır")
            .Matches(UserNameRegex).WithMessage("Kullanıcı adı sadece harf, rakam, alt çizgi veya tire içerebilir")
            .Must(NotContainWhitespace).WithMessage("Kullanıcı adı boşluk içeremez");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta gereklidir")
            .MaximumLength(256).WithMessage("E-posta en fazla 256 karakter olabilir")
            .Matches(EmailRegex).WithMessage("Geçersiz e-posta formatı")
            .EmailAddress().WithMessage("Geçersiz e-posta adresi");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre gereklidir")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır")
            .MaximumLength(128).WithMessage("Şifre en fazla 128 karakter olabilir")
            .Must(ContainUppercase).WithMessage("Şifre en az bir büyük harf içermelidir")
            .Must(ContainLowercase).WithMessage("Şifre en az bir küçük harf içermelidir")
            .Must(ContainDigit).WithMessage("Şifre en az bir rakam içermelidir")
            .Must(ContainSpecialCharacter).WithMessage("Şifre en az bir özel karakter içermelidir (!@#$%^&*()_+-=[]{}|;:,.<>?)");
    }

    private static bool NotContainWhitespace(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.Any(char.IsWhiteSpace);
    }

    private static bool ContainUppercase(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);
    }

    private static bool ContainLowercase(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsLower);
    }

    private static bool ContainDigit(string password)
    {
        return !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);
    }

    private static bool ContainSpecialCharacter(string password)
    {
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        return !string.IsNullOrEmpty(password) && password.Any(c => specialChars.Contains(c));
    }
}


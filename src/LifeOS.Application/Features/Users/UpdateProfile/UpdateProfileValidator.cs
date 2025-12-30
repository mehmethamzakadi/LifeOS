using FluentValidation;
using System.Text.RegularExpressions;

namespace LifeOS.Application.Features.Users.UpdateProfile;

public sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UserNameRegex = new(
        @"^[a-zA-Z0-9_-]{3,50}$",
        RegexOptions.Compiled);

    public UpdateProfileValidator()
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
    }

    private static bool NotContainWhitespace(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.Any(char.IsWhiteSpace);
    }
}


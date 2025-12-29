using LifeOS.Application.Common.Security;
using FluentValidation;

namespace LifeOS.Application.Features.Books.Commands.Update;

/// <summary>
/// Validator for UpdateBookCommand with security rules.
/// </summary>
public sealed class UpdateBookValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookValidator()
    {
        RuleFor(b => b.Id)
            .NotEmpty().WithMessage("Kitap ID'si boş olamaz!");

        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Kitap adı boş olmamalıdır!")
            .MinimumLength(2).WithMessage("Kitap adı en az 2 karakter olmalıdır!")
            .MaximumLength(200).WithMessage("Kitap adı en fazla 200 karakter olmalıdır!")
            .MustBePlainText("Kitap adı HTML veya script içeremez!");

        RuleFor(b => b.Author)
            .NotEmpty().WithMessage("Yazar adı boş olmamalıdır!")
            .MinimumLength(2).WithMessage("Yazar adı en az 2 karakter olmalıdır!")
            .MaximumLength(100).WithMessage("Yazar adı en fazla 100 karakter olmalıdır!")
            .MustBePlainText("Yazar adı HTML veya script içeremez!");

        RuleFor(b => b.CoverUrl)
            .MaximumLength(500).WithMessage("Kapak URL'si en fazla 500 karakter olabilir!")
            .When(b => !string.IsNullOrWhiteSpace(b.CoverUrl));

        RuleFor(b => b.TotalPages)
            .GreaterThanOrEqualTo(0).WithMessage("Toplam sayfa sayısı 0 veya daha büyük olmalıdır!");

        RuleFor(b => b.CurrentPage)
            .GreaterThanOrEqualTo(0).WithMessage("Mevcut sayfa sayısı 0 veya daha büyük olmalıdır!")
            .LessThanOrEqualTo(b => b.TotalPages).WithMessage("Mevcut sayfa sayısı toplam sayfa sayısından büyük olamaz!")
            .When(b => b.TotalPages > 0);

        RuleFor(b => b.Rating)
            .InclusiveBetween(1, 10).WithMessage("Değerlendirme 1 ile 10 arasında olmalıdır!")
            .When(b => b.Rating.HasValue);
    }
}


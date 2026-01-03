using FluentValidation;

namespace LifeOS.Application.Features.Books.GetBookByIsbn;

public sealed class GetBookByIsbnValidator : AbstractValidator<GetBookByIsbnQuery>
{
    public GetBookByIsbnValidator()
    {
        RuleFor(q => q.Isbn)
            .NotEmpty().WithMessage("ISBN numarası boş olamaz!")
            .MinimumLength(10).WithMessage("ISBN numarası en az 10 karakter olmalıdır!")
            .MaximumLength(17).WithMessage("ISBN numarası en fazla 17 karakter olabilir!")
            .Must(BeValidIsbnFormat).WithMessage("Geçersiz ISBN formatı! (Sadece rakamlar ve tire işareti kullanılabilir)");
    }

    private static bool BeValidIsbnFormat(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return false;
        }

        // ISBN formatı: 10 veya 13 haneli olabilir, tire içerebilir
        // Örnek: 978-0-123456-78-9 veya 0-123456-78-9 veya 9780123456789
        var cleaned = isbn.Replace("-", "").Replace(" ", "");
        
        // Sadece rakamlar ve son karakter X olabilir (ISBN-10 için)
        if (!System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^[\dX]{10,13}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            return false;
        }

        // En az 10, en fazla 13 karakter olmalı
        return cleaned.Length >= 10 && cleaned.Length <= 13;
    }
}


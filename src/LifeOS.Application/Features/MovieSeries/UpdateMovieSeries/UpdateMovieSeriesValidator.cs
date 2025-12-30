using FluentValidation;
using LifeOS.Application.Common.Security;

namespace LifeOS.Application.Features.MovieSeries.UpdateMovieSeries;

public sealed class UpdateMovieSeriesValidator : AbstractValidator<UpdateMovieSeriesCommand>
{
    public UpdateMovieSeriesValidator()
    {
        RuleFor(m => m.Id)
            .NotEmpty().WithMessage("Film/Dizi ID'si boş olamaz!");

        RuleFor(m => m.Title)
            .NotEmpty().WithMessage("Film/Dizi adı boş olmamalıdır!")
            .MinimumLength(2).WithMessage("Film/Dizi adı en az 2 karakter olmalıdır!")
            .MaximumLength(200).WithMessage("Film/Dizi adı en fazla 200 karakter olmalıdır!")
            .MustBePlainText("Film/Dizi adı HTML veya script içeremez!");

        RuleFor(m => m.CurrentSeason)
            .GreaterThan(0).WithMessage("Sezon numarası 0'dan büyük olmalıdır!")
            .When(m => m.CurrentSeason.HasValue);

        RuleFor(m => m.CurrentEpisode)
            .GreaterThan(0).WithMessage("Bölüm numarası 0'dan büyük olmalıdır!")
            .When(m => m.CurrentEpisode.HasValue);

        RuleFor(m => m.Rating)
            .InclusiveBetween(1, 10).WithMessage("Değerlendirme 1 ile 10 arasında olmalıdır!")
            .When(m => m.Rating.HasValue);

        RuleFor(m => m.PersonalNote)
            .MaximumLength(2000).WithMessage("Kişisel not en fazla 2000 karakter olabilir!")
            .When(m => !string.IsNullOrWhiteSpace(m.PersonalNote));
    }
}


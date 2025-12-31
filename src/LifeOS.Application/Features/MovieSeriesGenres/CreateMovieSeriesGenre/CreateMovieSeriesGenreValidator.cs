using FluentValidation;

namespace LifeOS.Application.Features.MovieSeriesGenres.CreateMovieSeriesGenre;

public sealed class CreateMovieSeriesGenreValidator : AbstractValidator<CreateMovieSeriesGenreCommand>
{
    public CreateMovieSeriesGenreValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tür adı boş olamaz")
            .MaximumLength(100).WithMessage("Tür adı en fazla 100 karakter olabilir");
    }
}


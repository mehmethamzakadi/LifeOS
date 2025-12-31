using FluentValidation;

namespace LifeOS.Application.Features.MovieSeriesGenres.UpdateMovieSeriesGenre;

public sealed class UpdateMovieSeriesGenreValidator : AbstractValidator<UpdateMovieSeriesGenreCommand>
{
    public UpdateMovieSeriesGenreValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Tür ID boş olamaz");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tür adı boş olamaz")
            .MaximumLength(100).WithMessage("Tür adı en fazla 100 karakter olabilir");
    }
}


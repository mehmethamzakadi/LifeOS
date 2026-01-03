using FluentValidation;

namespace LifeOS.Application.Features.Music.SaveTrack;

public sealed class SaveTrackValidator : AbstractValidator<SaveTrackCommand>
{
    public SaveTrackValidator()
    {
        RuleFor(c => c.SpotifyTrackId)
            .NotEmpty().WithMessage("Spotify track ID boş olamaz!");

        RuleFor(c => c.Notes)
            .MaximumLength(2000).WithMessage("Notlar en fazla 2000 karakter olabilir!")
            .When(c => !string.IsNullOrWhiteSpace(c.Notes));
    }
}


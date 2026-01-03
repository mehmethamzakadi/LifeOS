using FluentValidation;

namespace LifeOS.Application.Features.Music.GenerateMoodPlaylist;

public sealed class GenerateMoodPlaylistValidator : AbstractValidator<GenerateMoodPlaylistCommand>
{
    private static readonly HashSet<string> ValidMoods = new()
    {
        "mutlu", "üzgün", "enerjik", "sakin", "romantik", "nostaljik"
    };

    private static readonly HashSet<string> ValidLanguages = new()
    {
        "turkish", "foreign", "mixed"
    };

    public GenerateMoodPlaylistValidator()
    {
        RuleFor(x => x.Mood)
            .NotEmpty().WithMessage("Ruh hali seçilmelidir")
            .Must(mood => ValidMoods.Contains(mood.ToLowerInvariant()))
            .WithMessage($"Geçersiz ruh hali. Desteklenenler: {string.Join(", ", ValidMoods)}");

        RuleFor(x => x.LanguagePreference)
            .Must(lang => string.IsNullOrWhiteSpace(lang) || ValidLanguages.Contains(lang.ToLowerInvariant()))
            .WithMessage($"Geçersiz dil tercihi. Desteklenenler: {string.Join(", ", ValidLanguages)}");

        RuleFor(x => x.Limit)
            .InclusiveBetween(10, 50)
            .WithMessage("Playlist uzunluğu 10-50 arasında olmalıdır");
    }
}


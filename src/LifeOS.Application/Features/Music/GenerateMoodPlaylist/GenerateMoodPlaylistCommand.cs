namespace LifeOS.Application.Features.Music.GenerateMoodPlaylist;

public sealed record GenerateMoodPlaylistCommand(
    string Mood, // mutlu, üzgün, enerjik, sakin, romantik, nostaljik
    string LanguagePreference = "mixed", // turkish, foreign, mixed
    int Limit = 30
);


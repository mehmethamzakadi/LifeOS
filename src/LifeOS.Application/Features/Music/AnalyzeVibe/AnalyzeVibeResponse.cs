namespace LifeOS.Application.Features.Music.AnalyzeVibe;

public sealed record AnalyzeVibeResponse(
    string MoodTitle,
    string MoodIcon,
    int EnergyLevel, // 0-100
    int HappinessLevel, // 0-100 (Valence)
    int DanceabilityLevel, // 0-100
    string? TopGenre,
    int AnalyzedTracksCount
);


namespace LifeOS.Application.Features.Music.AnalyzeVibe;

public sealed record AnalyzeVibeQuery(
    string TimeRange = "short_term" // short_term, medium_term, long_term
);


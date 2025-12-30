using LifeOS.Domain.Common;
using LifeOS.Domain.Enums;
using LifeOS.Domain.Events.MovieSeriesEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Film/Dizi entity'si
/// </summary>
public sealed class MovieSeries : AggregateRoot
{
    // EF Core için parameterless constructor
    public MovieSeries() { }

    public string Title { get; private set; } = default!;
    public string? CoverUrl { get; private set; }
    public MovieSeriesType Type { get; private set; }
    public MovieSeriesPlatform Platform { get; private set; }
    public int? CurrentSeason { get; private set; }
    public int? CurrentEpisode { get; private set; }
    public MovieSeriesStatus Status { get; private set; }
    public int? Rating { get; private set; }
    public string? PersonalNote { get; private set; }

    public static MovieSeries Create(string title, string? coverUrl, MovieSeriesType type, MovieSeriesPlatform platform, int? currentSeason, int? currentEpisode, MovieSeriesStatus status, int? rating, string? personalNote)
    {
        var movieSeries = new MovieSeries
        {
            Id = Guid.NewGuid(),
            Title = title,
            CoverUrl = coverUrl,
            Type = type,
            Platform = platform,
            CurrentSeason = currentSeason,
            CurrentEpisode = currentEpisode,
            Status = status,
            Rating = rating,
            PersonalNote = personalNote,
            CreatedDate = DateTime.UtcNow
        };

        movieSeries.AddDomainEvent(new MovieSeriesCreatedEvent(movieSeries.Id, title));
        return movieSeries;
    }

    public void Update(string title, string? coverUrl, MovieSeriesType type, MovieSeriesPlatform platform, int? currentSeason, int? currentEpisode, MovieSeriesStatus status, int? rating, string? personalNote)
    {
        Title = title;
        CoverUrl = coverUrl;
        Type = type;
        Platform = platform;
        CurrentSeason = currentSeason;
        CurrentEpisode = currentEpisode;
        Status = status;
        Rating = rating;
        PersonalNote = personalNote;
        UpdatedDate = DateTime.UtcNow;

        AddDomainEvent(new MovieSeriesUpdatedEvent(Id, title));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new MovieSeriesDeletedEvent(Id, Title));
    }
}


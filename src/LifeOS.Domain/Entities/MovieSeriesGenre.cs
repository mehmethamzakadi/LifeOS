using LifeOS.Domain.Common;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Film/Dizi türü entity'si
/// </summary>
public sealed class MovieSeriesGenre : BaseEntity
{
    // EF Core için parameterless constructor
    public MovieSeriesGenre() { }

    public string Name { get; private set; } = default!;

    // Navigation properties
    public ICollection<MovieSeries> MovieSeries { get; private set; } = new List<MovieSeries>();

    public static MovieSeriesGenre Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("MovieSeriesGenre name cannot be empty");

        return new MovieSeriesGenre
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("MovieSeriesGenre name cannot be empty");

        Name = name;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("MovieSeriesGenre is already deleted");

        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
    }
}


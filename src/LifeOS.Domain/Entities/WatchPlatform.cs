using LifeOS.Domain.Common;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Film/Dizi izleme platformu entity'si
/// </summary>
public sealed class WatchPlatform : BaseEntity
{
    // EF Core için parameterless constructor
    public WatchPlatform() { }

    public string Name { get; private set; } = default!;

    // Navigation properties
    public ICollection<MovieSeries> MovieSeries { get; private set; } = new List<MovieSeries>();

    public static WatchPlatform Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("WatchPlatform name cannot be empty");

        return new WatchPlatform
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("WatchPlatform name cannot be empty");

        Name = name;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("WatchPlatform is already deleted");

        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
    }
}


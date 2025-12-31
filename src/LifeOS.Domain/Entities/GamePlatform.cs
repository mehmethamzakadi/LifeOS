using LifeOS.Domain.Common;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Oyun platformu entity'si
/// </summary>
public sealed class GamePlatform : BaseEntity
{
    // EF Core için parameterless constructor
    public GamePlatform() { }

    public string Name { get; private set; } = default!;

    // Navigation properties
    public ICollection<Game> Games { get; private set; } = new List<Game>();

    public static GamePlatform Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("GamePlatform name cannot be empty");

        return new GamePlatform
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("GamePlatform name cannot be empty");

        Name = name;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("GamePlatform is already deleted");

        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
    }
}


using LifeOS.Domain.Common;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Oyun mağazası entity'si
/// </summary>
public sealed class GameStore : BaseEntity
{
    // EF Core için parameterless constructor
    public GameStore() { }

    public string Name { get; private set; } = default!;

    // Navigation properties
    public ICollection<Game> Games { get; private set; } = new List<Game>();

    public static GameStore Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("GameStore name cannot be empty");

        return new GameStore
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedDate = DateTime.UtcNow
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.DomainValidationException("GameStore name cannot be empty");

        Name = name;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("GameStore is already deleted");

        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
    }
}


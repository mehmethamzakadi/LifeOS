using LifeOS.Domain.Common;
using LifeOS.Domain.Enums;
using LifeOS.Domain.Events.GameEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Oyun entity'si
/// </summary>
public sealed class Game : AggregateRoot
{
    // EF Core için parameterless constructor
    public Game() { }

    public string Title { get; private set; } = default!;
    public string? CoverUrl { get; private set; }
    public GamePlatform Platform { get; private set; }
    public GameStore Store { get; private set; }
    public GameStatus Status { get; private set; }
    public bool IsOwned { get; private set; }

    public static Game Create(string title, string? coverUrl, GamePlatform platform, GameStore store, GameStatus status, bool isOwned)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = title,
            CoverUrl = coverUrl,
            Platform = platform,
            Store = store,
            Status = status,
            IsOwned = isOwned,
            CreatedDate = DateTime.UtcNow
        };

        game.AddDomainEvent(new GameCreatedEvent(game.Id, title));
        return game;
    }

    public void Update(string title, string? coverUrl, GamePlatform platform, GameStore store, GameStatus status, bool isOwned)
    {
        Title = title;
        CoverUrl = coverUrl;
        Platform = platform;
        Store = store;
        Status = status;
        IsOwned = isOwned;
        UpdatedDate = DateTime.UtcNow;

        AddDomainEvent(new GameUpdatedEvent(Id, title));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new GameDeletedEvent(Id, Title));
    }
}


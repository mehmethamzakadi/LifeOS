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
    public Guid GamePlatformId { get; private set; }
    public Guid GameStoreId { get; private set; }
    public GameStatus Status { get; private set; }
    public bool IsOwned { get; private set; }

    // Navigation properties
    public GamePlatform GamePlatform { get; private set; } = null!;
    public GameStore GameStore { get; private set; } = null!;

    public static Game Create(string title, string? coverUrl, Guid gamePlatformId, Guid gameStoreId, GameStatus status, bool isOwned)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Title = title,
            CoverUrl = coverUrl,
            GamePlatformId = gamePlatformId,
            GameStoreId = gameStoreId,
            Status = status,
            IsOwned = isOwned,
            CreatedDate = DateTime.UtcNow
        };

        game.AddDomainEvent(new GameCreatedEvent(game.Id, title));
        return game;
    }

    public void Update(string title, string? coverUrl, Guid gamePlatformId, Guid gameStoreId, GameStatus status, bool isOwned)
    {
        Title = title;
        CoverUrl = coverUrl;
        GamePlatformId = gamePlatformId;
        GameStoreId = gameStoreId;
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


using LifeOS.Domain.Common;
using LifeOS.Domain.Enums;
using LifeOS.Domain.Events.GameEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Oyun entity'si
/// </summary>
public sealed class Game : BaseEntity
{
    // EF Core için parameterless constructor
    public Game() { }

    public string Title { get; set; } = default!;
    public string? CoverUrl { get; set; }
    public GamePlatform Platform { get; set; }
    public GameStore Store { get; set; }
    public GameStatus Status { get; set; }
    public bool IsOwned { get; set; }

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


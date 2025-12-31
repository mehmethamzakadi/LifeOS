using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.CreateGame;

public sealed record CreateGameCommand(
    string Title,
    string? CoverUrl = null,
    Guid GamePlatformId = default,
    Guid GameStoreId = default,
    GameStatus Status = GameStatus.Backlog,
    bool IsOwned = false);


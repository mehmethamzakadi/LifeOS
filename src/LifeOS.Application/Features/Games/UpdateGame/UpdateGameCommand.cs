using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.UpdateGame;

public sealed record UpdateGameCommand(
    Guid Id,
    string Title,
    string? CoverUrl = null,
    Guid GamePlatformId = default,
    Guid GameStoreId = default,
    GameStatus Status = GameStatus.Backlog,
    bool IsOwned = false);


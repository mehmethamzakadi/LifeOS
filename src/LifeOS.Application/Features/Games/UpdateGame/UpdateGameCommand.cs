using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.UpdateGame;

public sealed record UpdateGameCommand(
    Guid Id,
    string Title,
    string? CoverUrl = null,
    GamePlatform Platform = GamePlatform.PC,
    GameStore Store = GameStore.Steam,
    GameStatus Status = GameStatus.Backlog,
    bool IsOwned = false);


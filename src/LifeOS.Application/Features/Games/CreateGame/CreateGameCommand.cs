using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.CreateGame;

public sealed record CreateGameCommand(
    string Title,
    string? CoverUrl = null,
    GamePlatform Platform = GamePlatform.PC,
    GameStore Store = GameStore.Steam,
    GameStatus Status = GameStatus.Backlog,
    bool IsOwned = false);


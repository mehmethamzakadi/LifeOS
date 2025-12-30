using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.GetGameById;

public sealed record GetGameByIdResponse(
    Guid Id,
    string Title,
    string? CoverUrl,
    GamePlatform Platform,
    GameStore Store,
    GameStatus Status,
    bool IsOwned);


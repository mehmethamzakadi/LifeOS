using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.GetGameById;

public sealed record GetGameByIdResponse(
    Guid Id,
    string Title,
    string? CoverUrl,
    Guid GamePlatformId,
    string GamePlatformName,
    Guid GameStoreId,
    string GameStoreName,
    GameStatus Status,
    bool IsOwned);


using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.Queries.GetById;

public sealed record GetByIdGameResponse(
    Guid Id,
    string Title,
    string? CoverUrl,
    GamePlatform Platform,
    GameStore Store,
    GameStatus Status,
    bool IsOwned);


using LifeOS.Application.Common;
using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.SearchGames;

public sealed record SearchGamesResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public Guid GamePlatformId { get; init; }
    public string GamePlatformName { get; init; } = string.Empty;
    public Guid GameStoreId { get; init; }
    public string GameStoreName { get; init; } = string.Empty;
    public GameStatus Status { get; init; }
    public bool IsOwned { get; init; }
}


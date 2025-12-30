using LifeOS.Application.Common;
using LifeOS.Domain.Enums;

namespace LifeOS.Application.Features.Games.SearchGames;

public sealed record SearchGamesResponse : BaseEntityResponse
{
    public string Title { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public GamePlatform Platform { get; init; }
    public GameStore Store { get; init; }
    public GameStatus Status { get; init; }
    public bool IsOwned { get; init; }
}


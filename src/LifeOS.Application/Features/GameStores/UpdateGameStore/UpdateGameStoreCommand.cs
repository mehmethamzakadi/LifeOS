namespace LifeOS.Application.Features.GameStores.UpdateGameStore;

public sealed record UpdateGameStoreCommand(
    Guid Id,
    string Name);


using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

/// <summary>
/// Game repository interface
/// </summary>
public interface IGameRepository : IRepository<Game>
{
    /// <summary>
    /// Get game by ID
    /// </summary>
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}


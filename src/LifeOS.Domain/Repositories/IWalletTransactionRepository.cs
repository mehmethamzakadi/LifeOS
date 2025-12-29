using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

/// <summary>
/// WalletTransaction repository interface
/// </summary>
public interface IWalletTransactionRepository : IRepository<WalletTransaction>
{
    /// <summary>
    /// Get wallet transaction by ID
    /// </summary>
    Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}


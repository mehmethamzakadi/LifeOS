using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

/// <summary>
/// Book repository interface
/// </summary>
public interface IBookRepository : IRepository<Book>
{
    /// <summary>
    /// Get book by ID
    /// </summary>
    Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}


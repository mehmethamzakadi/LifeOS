using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

/// <summary>
/// PersonalNote repository interface
/// </summary>
public interface IPersonalNoteRepository : IRepository<PersonalNote>
{
    /// <summary>
    /// Get personal note by ID
    /// </summary>
    Task<PersonalNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}


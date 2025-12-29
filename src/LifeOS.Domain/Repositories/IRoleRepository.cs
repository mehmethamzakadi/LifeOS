using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

/// <summary>
/// Role repository interface - specific queries to avoid IQueryable leaks
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    Task<Paginate<Role>> GetRoles(int index, int size, CancellationToken cancellationToken);
    Task<Role?> FindByNameAsync(string roleName);
    bool AnyRole(string name);

    /// <summary>
    /// Get roles by IDs
    /// </summary>
    Task<List<Role>> GetByIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count total roles
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

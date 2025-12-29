using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

public sealed class RoleRepository(LifeOSDbContext context) : EfRepositoryBase<Role, LifeOSDbContext>(context), IRoleRepository
{

    public async Task<Paginate<Role>> GetRoles(int index, int size, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        return await Context.Roles
            .AsNoTracking()
            .ToPaginateAsync(index, size, cancellationToken);
    }


    public async Task<Role?> FindByNameAsync(string roleName)
    {
        // ✅ Validation için kullanılıyor - tracking'e gerek yok (performans için)
        // ✅ NormalizedName üzerinden case-insensitive karşılaştırma
        var normalizedName = roleName.ToUpperInvariant();
        return await Context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);
    }

    public bool AnyRole(string name)
    {
        // ✅ NormalizedName üzerinden case-insensitive karşılaştırma
        var normalizedName = name.ToUpperInvariant();
        var result = Context.Roles
            .Any(x => x.NormalizedName == normalizedName);

        return result;
    }

    public async Task<List<Role>> GetByIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        return await Query()
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Query().CountAsync(cancellationToken);
    }
}

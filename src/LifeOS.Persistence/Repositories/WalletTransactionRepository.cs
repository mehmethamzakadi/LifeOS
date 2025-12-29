using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

/// <summary>
/// WalletTransaction repository implementation
/// </summary>
public class WalletTransactionRepository(LifeOSDbContext dbContext) : EfRepositoryBase<WalletTransaction, LifeOSDbContext>(dbContext), IWalletTransactionRepository
{
    public async Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }
}


using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

/// <summary>
/// Game repository implementation
/// </summary>
public class GameRepository(LifeOSDbContext dbContext) : EfRepositoryBase<Game, LifeOSDbContext>(dbContext), IGameRepository
{
    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}


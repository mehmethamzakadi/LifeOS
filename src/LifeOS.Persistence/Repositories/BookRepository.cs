using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

/// <summary>
/// Book repository implementation
/// </summary>
public class BookRepository(LifeOSDbContext dbContext) : EfRepositoryBase<Book, LifeOSDbContext>(dbContext), IBookRepository
{
    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }
}


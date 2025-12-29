using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

/// <summary>
/// PersonalNote repository implementation
/// </summary>
public class PersonalNoteRepository(LifeOSDbContext dbContext) : EfRepositoryBase<PersonalNote, LifeOSDbContext>(dbContext), IPersonalNoteRepository
{
    public async Task<PersonalNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}


using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Persistence.Repositories;

/// <summary>
/// MovieSeries repository implementation
/// </summary>
public class MovieSeriesRepository(LifeOSDbContext dbContext) : EfRepositoryBase<MovieSeries, LifeOSDbContext>(dbContext), IMovieSeriesRepository
{
    public async Task<MovieSeries?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }
}


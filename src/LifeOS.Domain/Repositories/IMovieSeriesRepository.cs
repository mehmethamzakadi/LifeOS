using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;

namespace LifeOS.Domain.Repositories;

/// <summary>
/// MovieSeries repository interface
/// </summary>
public interface IMovieSeriesRepository : IRepository<MovieSeries>
{
    /// <summary>
    /// Get movie/series by ID
    /// </summary>
    Task<MovieSeries?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}


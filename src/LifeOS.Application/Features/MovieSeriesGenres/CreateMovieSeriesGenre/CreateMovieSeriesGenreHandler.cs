using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeriesGenres.CreateMovieSeriesGenre;

public sealed class CreateMovieSeriesGenreHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateMovieSeriesGenreHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateMovieSeriesGenreResponse> HandleAsync(
        CreateMovieSeriesGenreCommand command,
        CancellationToken cancellationToken)
    {
        bool genreExists = await _context.MovieSeriesGenres
            .AnyAsync(x => x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (genreExists)
        {
            throw new InvalidOperationException("Bu tür adı zaten mevcut!");
        }

        var genre = MovieSeriesGenre.Create(command.Name);
        await _context.MovieSeriesGenres.AddAsync(genre, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.MovieSeriesGenreListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new CreateMovieSeriesGenreResponse(genre.Id);
    }
}


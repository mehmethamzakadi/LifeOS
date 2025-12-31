using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeriesGenres.UpdateMovieSeriesGenre;

public sealed class UpdateMovieSeriesGenreHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateMovieSeriesGenreHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        UpdateMovieSeriesGenreCommand command,
        CancellationToken cancellationToken)
    {
        var genre = await _context.MovieSeriesGenres
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (genre is null)
            return ApiResultExtensions.Failure("Film/Dizi türü bulunamadı");

        // Aynı isimde başka bir tür var mı kontrol et
        bool nameExists = await _context.MovieSeriesGenres
            .AnyAsync(x => x.Id != command.Id && x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (nameExists)
            return ApiResultExtensions.Failure("Bu tür adı zaten kullanılıyor");

        genre.Update(command.Name);
        _context.MovieSeriesGenres.Update(genre);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.MovieSeriesGenreListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("Film/Dizi türü başarıyla güncellendi");
    }
}


using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeriesGenres.DeleteMovieSeriesGenre;

public sealed class DeleteMovieSeriesGenreHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteMovieSeriesGenreHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var genre = await _context.MovieSeriesGenres
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (genre is null)
            return ApiResultExtensions.Failure("Film/Dizi türü bulunamadı");

        // Kullanımda mı kontrol et
        var isInUse = await _context.MovieSeries
            .AnyAsync(x => x.MovieSeriesGenreId == id && !x.IsDeleted, cancellationToken);

        if (isInUse)
            return ApiResultExtensions.Failure("Bu tür kullanımda olduğu için silinemez");

        genre.Delete();
        _context.MovieSeriesGenres.Update(genre);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Add(
            CacheKeys.MovieSeriesGenreListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("Film/Dizi türü başarıyla silindi");
    }
}


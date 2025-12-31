using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeriesGenres.GetAllMovieSeriesGenres;

public sealed class GetAllMovieSeriesGenresHandler
{
    private readonly LifeOSDbContext _context;

    public GetAllMovieSeriesGenresHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<List<GetAllMovieSeriesGenresResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var genres = await _context.MovieSeriesGenres
            .Where(g => !g.IsDeleted)
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);

        var response = genres.Select(g => new GetAllMovieSeriesGenresResponse(g.Id, g.Name)).ToList();

        return ApiResultExtensions.Success(response, "Film/Dizi türleri başarıyla getirildi");
    }
}


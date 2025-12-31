using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.GetRecentActivities;

public sealed class GetRecentActivitiesHandler
{
    private readonly LifeOSDbContext _context;

    public GetRecentActivitiesHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetRecentActivitiesResponse>> HandleAsync(
        CancellationToken cancellationToken)
    {
        // Son 5 kitap
        var recentBooks = await _context.Books
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.CreatedDate)
            .Take(5)
            .Select(b => new RecentActivityItem(
                b.Id,
                "book",
                b.Title,
                b.Author,
                b.CreatedDate))
            .ToListAsync(cancellationToken);

        // Son 5 oyun
        var recentGames = await _context.Games
            .Include(g => g.GamePlatform)
            .Where(g => !g.IsDeleted)
            .OrderByDescending(g => g.CreatedDate)
            .Take(5)
            .Select(g => new RecentActivityItem(
                g.Id,
                "game",
                g.Title,
                g.GamePlatform.Name,
                g.CreatedDate))
            .ToListAsync(cancellationToken);

        // Son 5 film/dizi
        var recentMovies = await _context.MovieSeries
            .Include(m => m.Genre)
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedDate)
            .Take(5)
            .Select(m => new RecentActivityItem(
                m.Id,
                "movie",
                m.Title,
                m.Genre.Name,
                m.CreatedDate))
            .ToListAsync(cancellationToken);

        // Son 5 not
        var recentNotes = await _context.PersonalNotes
            .Where(n => !n.IsDeleted)
            .OrderByDescending(n => n.CreatedDate)
            .Take(5)
            .Select(n => new RecentActivityItem(
                n.Id,
                "note",
                n.Title,
                n.Category,
                n.CreatedDate))
            .ToListAsync(cancellationToken);

        // Son 5 cüzdan işlemi
        var recentWalletTransactions = await _context.WalletTransactions
            .Where(w => !w.IsDeleted)
            .OrderByDescending(w => w.CreatedDate)
            .Take(5)
            .Select(w => new RecentActivityItem(
                w.Id,
                "wallet",
                w.Title,
                $"{w.Type} - {w.Amount:C}",
                w.CreatedDate))
            .ToListAsync(cancellationToken);

        var response = new GetRecentActivitiesResponse(
            recentBooks,
            recentGames,
            recentMovies,
            recentNotes,
            recentWalletTransactions);

        return ApiResultExtensions.Success(response, "Son aktiviteler başarıyla getirildi");
    }
}

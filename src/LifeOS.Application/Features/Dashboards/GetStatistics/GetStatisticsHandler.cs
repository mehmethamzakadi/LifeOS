using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.GetStatistics;

public sealed class GetStatisticsHandler
{
    private readonly LifeOSDbContext _context;

    public GetStatisticsHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetStatisticsResponse>> HandleAsync(
        CancellationToken cancellationToken)
    {
        // Temel istatistikler
        var totalCategories = await _context.Categories
            .CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalUsers = await _context.Users
            .CountAsync(u => !u.IsDeleted, cancellationToken);
        var totalRoles = await _context.Roles
            .CountAsync(r => !r.IsDeleted, cancellationToken);

        // Kitap istatistikleri
        var totalBooks = await _context.Books
            .CountAsync(b => !b.IsDeleted, cancellationToken);
        var totalBooksReading = await _context.Books
            .CountAsync(b => !b.IsDeleted && b.Status == BookStatus.Reading, cancellationToken);
        var totalBooksCompleted = await _context.Books
            .CountAsync(b => !b.IsDeleted && b.Status == BookStatus.Completed, cancellationToken);

        // Oyun istatistikleri
        var totalGames = await _context.Games
            .CountAsync(g => !g.IsDeleted, cancellationToken);
        var totalGamesPlaying = await _context.Games
            .CountAsync(g => !g.IsDeleted && g.Status == GameStatus.Playing, cancellationToken);
        var totalGamesCompleted = await _context.Games
            .CountAsync(g => !g.IsDeleted && g.Status == GameStatus.Completed, cancellationToken);

        // Film/Dizi istatistikleri
        var totalMovies = await _context.MovieSeries
            .CountAsync(m => !m.IsDeleted, cancellationToken);
        var totalMoviesWatching = await _context.MovieSeries
            .CountAsync(m => !m.IsDeleted && m.Status == MovieSeriesStatus.Watching, cancellationToken);
        var totalMoviesCompleted = await _context.MovieSeries
            .CountAsync(m => !m.IsDeleted && m.Status == MovieSeriesStatus.Completed, cancellationToken);

        // Not istatistikleri
        var totalNotes = await _context.PersonalNotes
            .CountAsync(n => !n.IsDeleted, cancellationToken);

        // Cüzdan istatistikleri - Tüm zamanlar
        var totalIncome = await _context.WalletTransactions
            .Where(w => !w.IsDeleted && w.Type == TransactionType.Income)
            .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);
        var totalExpense = await _context.WalletTransactions
            .Where(w => !w.IsDeleted && w.Type == TransactionType.Expense)
            .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

        // Cüzdan istatistikleri - Bu ay
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var currentMonthEnd = currentMonthStart.AddMonths(1);

        var currentMonthIncome = await _context.WalletTransactions
            .Where(w => !w.IsDeleted 
                && w.Type == TransactionType.Income 
                && w.TransactionDate >= currentMonthStart 
                && w.TransactionDate < currentMonthEnd)
            .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

        var currentMonthExpense = await _context.WalletTransactions
            .Where(w => !w.IsDeleted 
                && w.Type == TransactionType.Expense 
                && w.TransactionDate >= currentMonthStart 
                && w.TransactionDate < currentMonthEnd)
            .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

        var response = new GetStatisticsResponse(
            totalCategories,
            totalUsers,
            totalRoles,
            totalBooks,
            totalBooksReading,
            totalBooksCompleted,
            totalGames,
            totalGamesPlaying,
            totalGamesCompleted,
            totalMovies,
            totalMoviesWatching,
            totalMoviesCompleted,
            totalNotes,
            totalIncome,
            totalExpense,
            currentMonthIncome,
            currentMonthExpense);

        return ApiResultExtensions.Success(response, "İstatistikler başarıyla getirildi");
    }
}


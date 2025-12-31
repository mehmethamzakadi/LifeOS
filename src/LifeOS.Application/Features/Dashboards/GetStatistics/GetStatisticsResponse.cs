namespace LifeOS.Application.Features.Dashboards.GetStatistics;

public sealed record GetStatisticsResponse(
    int TotalCategories,
    int TotalUsers,
    int TotalRoles,
    int TotalBooks,
    int TotalBooksReading,
    int TotalBooksCompleted,
    int TotalGames,
    int TotalGamesPlaying,
    int TotalGamesCompleted,
    int TotalMovies,
    int TotalMoviesWatching,
    int TotalMoviesCompleted,
    int TotalNotes,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal CurrentMonthIncome,
    decimal CurrentMonthExpense);


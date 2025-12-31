using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Dashboards.GetFinancialSummary;

public sealed class GetFinancialSummaryHandler
{
    private readonly LifeOSDbContext _context;

    public GetFinancialSummaryHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetFinancialSummaryResponse>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var currentMonthEnd = currentMonthStart.AddMonths(1);

        // Bu ay istatistikleri
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

        var currentMonthNet = currentMonthIncome - currentMonthExpense;

        // Tüm zamanlar istatistikleri
        var totalIncome = await _context.WalletTransactions
            .Where(w => !w.IsDeleted && w.Type == TransactionType.Income)
            .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

        var totalExpense = await _context.WalletTransactions
            .Where(w => !w.IsDeleted && w.Type == TransactionType.Expense)
            .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

        var totalNet = totalIncome - totalExpense;

        // Son 6 ay verileri
        var last6Months = new List<MonthlyFinancialData>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = currentMonthStart.AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var monthName = monthStart.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));

            var monthIncome = await _context.WalletTransactions
                .Where(w => !w.IsDeleted
                    && w.Type == TransactionType.Income
                    && w.TransactionDate >= monthStart
                    && w.TransactionDate < monthEnd)
                .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

            var monthExpense = await _context.WalletTransactions
                .Where(w => !w.IsDeleted
                    && w.Type == TransactionType.Expense
                    && w.TransactionDate >= monthStart
                    && w.TransactionDate < monthEnd)
                .SumAsync(w => (decimal?)w.Amount ?? 0, cancellationToken);

            last6Months.Add(new MonthlyFinancialData(
                monthName,
                monthIncome,
                monthExpense,
                monthIncome - monthExpense));
        }

        // En çok harcama yapılan kategoriler (son 6 ay)
        var sixMonthsAgo = currentMonthStart.AddMonths(-6);
        var expenseTransactions = await _context.WalletTransactions
            .Where(w => !w.IsDeleted
                && w.Type == TransactionType.Expense
                && w.TransactionDate >= sixMonthsAgo)
            .Select(w => new { w.Category, w.Amount })
            .ToListAsync(cancellationToken);

        var topExpenseCategories = expenseTransactions
            .GroupBy(w => w.Category)
            .Select(g => new CategoryExpense(
                g.Key.ToString(),
                g.Sum(w => w.Amount),
                g.Count()))
            .OrderByDescending(c => c.Amount)
            .Take(5)
            .ToList();

        var response = new GetFinancialSummaryResponse(
            currentMonthIncome,
            currentMonthExpense,
            currentMonthNet,
            totalIncome,
            totalExpense,
            totalNet,
            last6Months,
            topExpenseCategories);

        return ApiResultExtensions.Success(response, "Finansal özet başarıyla getirildi");
    }
}

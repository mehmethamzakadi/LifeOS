namespace LifeOS.Application.Features.Dashboards.GetFinancialSummary;

public sealed record MonthlyFinancialData(
    string Month,
    decimal Income,
    decimal Expense,
    decimal Net);

public sealed record CategoryExpense(
    string Category,
    decimal Amount,
    int Count);

public sealed record GetFinancialSummaryResponse(
    decimal CurrentMonthIncome,
    decimal CurrentMonthExpense,
    decimal CurrentMonthNet,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal TotalNet,
    List<MonthlyFinancialData> Last6Months,
    List<CategoryExpense> TopExpenseCategories);


import api from "../../lib/axios";
import { ApiResult, normalizeApiResult } from "../../types/api";

export interface StatisticsResponse {
  totalCategories: number;
  totalUsers: number;
  totalRoles: number;
  totalBooks: number;
  totalBooksReading: number;
  totalBooksCompleted: number;
  totalGames: number;
  totalGamesPlaying: number;
  totalGamesCompleted: number;
  totalMovies: number;
  totalMoviesWatching: number;
  totalMoviesCompleted: number;
  totalNotes: number;
  totalIncome: number;
  totalExpense: number;
  currentMonthIncome: number;
  currentMonthExpense: number;
}

export interface RecentActivityItem {
  id: string;
  type: string;
  title: string;
  subtitle: string | null;
  createdDate: string;
}

export interface RecentActivitiesResponse {
  books: RecentActivityItem[];
  games: RecentActivityItem[];
  movies: RecentActivityItem[];
  notes: RecentActivityItem[];
  walletTransactions: RecentActivityItem[];
}

export interface MonthlyFinancialData {
  month: string;
  income: number;
  expense: number;
  net: number;
}

export interface CategoryExpense {
  category: string;
  amount: number;
  count: number;
}

export interface FinancialSummaryResponse {
  currentMonthIncome: number;
  currentMonthExpense: number;
  currentMonthNet: number;
  totalIncome: number;
  totalExpense: number;
  totalNet: number;
  last6Months: MonthlyFinancialData[];
  topExpenseCategories: CategoryExpense[];
}

export async function fetchStatistics(): Promise<StatisticsResponse> {
  const response = await api.get<ApiResult<StatisticsResponse>>("/dashboards/statistics");
  const result = normalizeApiResult<StatisticsResponse>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'İstatistikler alınamadı');
  }
  return result.data;
}

export async function fetchRecentActivities(): Promise<RecentActivitiesResponse> {
  const response = await api.get<ApiResult<RecentActivitiesResponse>>("/dashboards/recent-activities");
  const result = normalizeApiResult<RecentActivitiesResponse>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Son aktiviteler alınamadı');
  }
  return result.data;
}

// Helper function to normalize nested objects from PascalCase to camelCase
function normalizeFinancialData(data: any): FinancialSummaryResponse | null {
  if (!data) return null;
  
  return {
    currentMonthIncome: data.currentMonthIncome ?? data.CurrentMonthIncome ?? 0,
    currentMonthExpense: data.currentMonthExpense ?? data.CurrentMonthExpense ?? 0,
    currentMonthNet: data.currentMonthNet ?? data.CurrentMonthNet ?? 0,
    totalIncome: data.totalIncome ?? data.TotalIncome ?? 0,
    totalExpense: data.totalExpense ?? data.TotalExpense ?? 0,
    totalNet: data.totalNet ?? data.TotalNet ?? 0,
    last6Months: (data.last6Months ?? data.Last6Months ?? []).map((month: any) => ({
      month: month.month ?? month.Month ?? "",
      income: month.income ?? month.Income ?? 0,
      expense: month.expense ?? month.Expense ?? 0,
      net: month.net ?? month.Net ?? 0,
    })),
    topExpenseCategories: (data.topExpenseCategories ?? data.TopExpenseCategories ?? []).map((cat: any) => ({
      category: cat.category ?? cat.Category ?? "",
      amount: cat.amount ?? cat.Amount ?? 0,
      count: cat.count ?? cat.Count ?? 0,
    })),
  };
}

export async function fetchFinancialSummary(): Promise<FinancialSummaryResponse> {
  const response = await api.get<ApiResult<FinancialSummaryResponse>>("/dashboards/financial-summary");
  const result = normalizeApiResult<FinancialSummaryResponse>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Finansal özet alınamadı');
  }
  // Normalize nested objects from PascalCase to camelCase
  const normalized = normalizeFinancialData(result.data);
  if (!normalized) {
    throw new Error('Finansal özet verisi normalize edilemedi');
  }
  return normalized;
}

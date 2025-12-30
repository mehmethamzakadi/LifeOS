import api from "../../lib/axios";
import { ApiResult, normalizeApiResult } from "../../types/api";

export interface StatisticsResponse {
  totalCategories: number;
  totalUsers: number;
  totalRoles: number;
}

export async function fetchStatistics(): Promise<StatisticsResponse> {
  const response = await api.get<ApiResult<StatisticsResponse>>("/dashboards/statistics");
  const result = normalizeApiResult<StatisticsResponse>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'İstatistikler alınamadı');
  }
  return result.data;
}

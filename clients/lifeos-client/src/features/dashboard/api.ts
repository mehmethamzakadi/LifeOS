import api from "../../lib/axios";

export interface StatisticsResponse {
  totalCategories: number;
  totalUsers: number;
  totalRoles: number;
}

export async function fetchStatistics(): Promise<StatisticsResponse> {
  const response = await api.get<StatisticsResponse>("/dashboards/statistics");
  return response.data;
}

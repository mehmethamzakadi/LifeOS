import api from "../../lib/axios";

export interface StatisticsResponse {
  totalCategories: number;
  totalUsers: number;
  totalRoles: number;
}

export interface ActivityLogItem {
  id: string;
  activityType: string;
  title: string;
  timestamp: string;
  userName?: string;
}

export async function fetchStatistics(): Promise<StatisticsResponse> {
  const response = await api.get<StatisticsResponse>("/dashboards/statistics");
  return response.data;
}

export async function fetchRecentActivities(
  count: number = 10
): Promise<ActivityLogItem[]> {
  const response = await api.get<{ activities: ActivityLogItem[] }>(
    `/dashboards/activities?count=${count}`
  );
  return response.data.activities || [];
}

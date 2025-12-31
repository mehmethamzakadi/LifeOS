import api from '../../lib/axios';
import { ApiResult, normalizeApiResult } from '../../types/api';
import { WatchPlatform, WatchPlatformFormValues } from './types';

export async function getAllWatchPlatforms(): Promise<WatchPlatform[]> {
  const response = await api.get<ApiResult<WatchPlatform[]>>('/watch-platforms');
  const result = normalizeApiResult<WatchPlatform[]>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'İzleme platformları getirilemedi');
  }
  return result.data;
}

export async function createWatchPlatform(values: WatchPlatformFormValues) {
  const response = await api.post<ApiResult>('/watch-platforms', {
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function updateWatchPlatform(id: string, values: WatchPlatformFormValues) {
  const response = await api.put<ApiResult>(`/watch-platforms/${id}`, {
    Id: id,
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function deleteWatchPlatform(id: string) {
  const response = await api.delete<ApiResult>(`/watch-platforms/${id}`);
  return normalizeApiResult(response.data);
}


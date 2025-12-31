import api from '../../lib/axios';
import { ApiResult, normalizeApiResult } from '../../types/api';
import { GamePlatform, GamePlatformFormValues } from './types';

export async function getAllGamePlatforms(): Promise<GamePlatform[]> {
  const response = await api.get<ApiResult<GamePlatform[]>>('/game-platforms');
  const result = normalizeApiResult<GamePlatform[]>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Oyun platformları getirilemedi');
  }
  return result.data;
}

export async function createGamePlatform(values: GamePlatformFormValues) {
  const response = await api.post<ApiResult>('/game-platforms', {
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function updateGamePlatform(id: string, values: GamePlatformFormValues) {
  const response = await api.put<ApiResult>(`/game-platforms/${id}`, {
    Id: id,
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function deleteGamePlatform(id: string) {
  const response = await api.delete<ApiResult>(`/game-platforms/${id}`);
  return normalizeApiResult(response.data);
}


import api from '../../lib/axios';
import { ApiResult, normalizeApiResult } from '../../types/api';
import { GameStore, GameStoreFormValues } from './types';

export async function getAllGameStores(): Promise<GameStore[]> {
  const response = await api.get<ApiResult<GameStore[]>>('/game-stores');
  const result = normalizeApiResult<GameStore[]>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Oyun mağazaları getirilemedi');
  }
  return result.data;
}

export async function createGameStore(values: GameStoreFormValues) {
  const response = await api.post<ApiResult>('/game-stores', {
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function updateGameStore(id: string, values: GameStoreFormValues) {
  const response = await api.put<ApiResult>(`/game-stores/${id}`, {
    Id: id,
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function deleteGameStore(id: string) {
  const response = await api.delete<ApiResult>(`/game-stores/${id}`);
  return normalizeApiResult(response.data);
}


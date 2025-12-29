import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import { Game, GameFormValues, GameListResponse, GameTableFilters } from './types';

export async function fetchGames(filters: GameTableFilters): Promise<GameListResponse> {
  const response = await api.post('/game/search', buildDataGridPayload(filters, 'Title'));
  return normalizePaginatedResponse<Game>(response.data);
}

export async function getGameById(id: string): Promise<Game> {
  const response = await api.get<ApiResult<Game>>(`/game/${id}`);
  const result = normalizeApiResult<Game>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Oyun bulunamadı');
  }
  return result.data;
}

export async function createGame(values: GameFormValues) {
  const response = await api.post<ApiResult>('/game', {
    Title: values.title,
    CoverUrl: values.coverUrl || null,
    Platform: values.platform,
    Store: values.store,
    Status: values.status,
    IsOwned: values.isOwned
  });
  return normalizeApiResult(response.data);
}

export async function updateGame(id: string, values: GameFormValues) {
  const response = await api.put<ApiResult>(`/game/${id}`, {
    Id: id,
    Title: values.title,
    CoverUrl: values.coverUrl || null,
    Platform: values.platform,
    Store: values.store,
    Status: values.status,
    IsOwned: values.isOwned
  });
  return normalizeApiResult(response.data);
}

export async function deleteGame(id: string) {
  const response = await api.delete<ApiResult>(`/game/${id}`);
  return normalizeApiResult(response.data);
}


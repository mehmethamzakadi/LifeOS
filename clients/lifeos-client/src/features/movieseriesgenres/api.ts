import api from '../../lib/axios';
import { ApiResult, normalizeApiResult } from '../../types/api';
import { MovieSeriesGenre, MovieSeriesGenreFormValues } from './types';

export async function getAllMovieSeriesGenres(): Promise<MovieSeriesGenre[]> {
  const response = await api.get<ApiResult<MovieSeriesGenre[]>>('/movie-series-genres');
  const result = normalizeApiResult<MovieSeriesGenre[]>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Film/Dizi türleri getirilemedi');
  }
  return result.data;
}

export async function createMovieSeriesGenre(values: MovieSeriesGenreFormValues) {
  const response = await api.post<ApiResult>('/movie-series-genres', {
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function updateMovieSeriesGenre(id: string, values: MovieSeriesGenreFormValues) {
  const response = await api.put<ApiResult>(`/movie-series-genres/${id}`, {
    Id: id,
    Name: values.name
  });
  return normalizeApiResult(response.data);
}

export async function deleteMovieSeriesGenre(id: string) {
  const response = await api.delete<ApiResult>(`/movie-series-genres/${id}`);
  return normalizeApiResult(response.data);
}


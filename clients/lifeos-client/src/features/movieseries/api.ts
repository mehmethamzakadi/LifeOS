import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import { MovieSeries, MovieSeriesFormValues, MovieSeriesListResponse, MovieSeriesTableFilters } from './types';

export async function fetchMovieSeries(filters: MovieSeriesTableFilters): Promise<MovieSeriesListResponse> {
  const response = await api.post('/movieseries/search', buildDataGridPayload(filters, 'Title'));
  return normalizePaginatedResponse<MovieSeries>(response.data);
}

export async function getMovieSeriesById(id: string): Promise<MovieSeries> {
  const response = await api.get<ApiResult<MovieSeries>>(`/movieseries/${id}`);
  const result = normalizeApiResult<MovieSeries>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Film/Dizi bulunamadı');
  }
  return result.data;
}

export async function createMovieSeries(values: MovieSeriesFormValues) {
  const response = await api.post<ApiResult>('/movieseries', {
    Title: values.title,
    CoverUrl: values.coverUrl || null,
    MovieSeriesGenreId: values.movieSeriesGenreId,
    WatchPlatformId: values.watchPlatformId,
    CurrentSeason: values.currentSeason || null,
    CurrentEpisode: values.currentEpisode || null,
    Status: values.status,
    Rating: values.rating || null,
    PersonalNote: values.personalNote || null
  });
  return normalizeApiResult(response.data);
}

export async function updateMovieSeries(id: string, values: MovieSeriesFormValues) {
  const response = await api.put<ApiResult>(`/movieseries/${id}`, {
    Id: id,
    Title: values.title,
    CoverUrl: values.coverUrl || null,
    MovieSeriesGenreId: values.movieSeriesGenreId,
    WatchPlatformId: values.watchPlatformId,
    CurrentSeason: values.currentSeason || null,
    CurrentEpisode: values.currentEpisode || null,
    Status: values.status,
    Rating: values.rating || null,
    PersonalNote: values.personalNote || null
  });
  return normalizeApiResult(response.data);
}

export async function deleteMovieSeries(id: string) {
  const response = await api.delete<ApiResult>(`/movieseries/${id}`);
  return normalizeApiResult(response.data);
}


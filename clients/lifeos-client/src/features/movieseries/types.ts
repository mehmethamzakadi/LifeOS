import { PaginatedListResponse } from '../../types/api';

export enum MovieSeriesStatus {
  ToWatch = 0,
  Watching = 1,
  Completed = 2
}

export interface MovieSeries {
  id: string;
  title: string;
  coverUrl?: string;
  movieSeriesGenreId: string;
  movieSeriesGenreName: string;
  watchPlatformId: string;
  watchPlatformName: string;
  currentSeason?: number;
  currentEpisode?: number;
  status: MovieSeriesStatus;
  rating?: number;
  personalNote?: string;
  createdDate: string;
}

export type MovieSeriesListResponse = PaginatedListResponse<MovieSeries>;

export interface MovieSeriesFormValues {
  title: string;
  coverUrl?: string;
  movieSeriesGenreId: string;
  watchPlatformId: string;
  currentSeason?: number;
  currentEpisode?: number;
  status: MovieSeriesStatus;
  rating?: number;
  personalNote?: string;
}

export interface MovieSeriesTableFilters {
  search?: string;
  pageIndex: number;
  pageSize: number;
  sort?: {
    field: string;
    dir: 'asc' | 'desc';
  };
  movieSeriesGenreId?: string;
  watchPlatformId?: string;
  status?: MovieSeriesStatus;
}

export const MovieSeriesStatusLabels: Record<MovieSeriesStatus, string> = {
  [MovieSeriesStatus.ToWatch]: 'İzlenecek',
  [MovieSeriesStatus.Watching]: 'İzleniyor',
  [MovieSeriesStatus.Completed]: 'Tamamlandı'
};


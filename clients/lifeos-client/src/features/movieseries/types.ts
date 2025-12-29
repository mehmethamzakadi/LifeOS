import { PaginatedListResponse } from '../../types/api';

export enum MovieSeriesType {
  Movie = 0,
  Series = 1
}

export enum MovieSeriesPlatform {
  Netflix = 0,
  Prime = 1,
  Disney = 2,
  Local = 3
}

export enum MovieSeriesStatus {
  ToWatch = 0,
  Watching = 1,
  Completed = 2
}

export interface MovieSeries {
  id: string;
  title: string;
  coverUrl?: string;
  type: MovieSeriesType;
  platform: MovieSeriesPlatform;
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
  type: MovieSeriesType;
  platform: MovieSeriesPlatform;
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
  type?: MovieSeriesType;
  platform?: MovieSeriesPlatform;
  status?: MovieSeriesStatus;
}

export const MovieSeriesTypeLabels: Record<MovieSeriesType, string> = {
  [MovieSeriesType.Movie]: 'Film',
  [MovieSeriesType.Series]: 'Dizi'
};

export const MovieSeriesPlatformLabels: Record<MovieSeriesPlatform, string> = {
  [MovieSeriesPlatform.Netflix]: 'Netflix',
  [MovieSeriesPlatform.Prime]: 'Prime Video',
  [MovieSeriesPlatform.Disney]: 'Disney+',
  [MovieSeriesPlatform.Local]: 'Yerel'
};

export const MovieSeriesStatusLabels: Record<MovieSeriesStatus, string> = {
  [MovieSeriesStatus.ToWatch]: 'İzlenecek',
  [MovieSeriesStatus.Watching]: 'İzleniyor',
  [MovieSeriesStatus.Completed]: 'Tamamlandı'
};


import { PaginatedListResponse } from '../../types/api';

export enum BookStatus {
  ToRead = 0,
  Reading = 1,
  Completed = 2,
  Dropped = 3
}

export interface Book {
  id: string;
  title: string;
  author: string;
  coverUrl?: string;
  totalPages: number;
  currentPage: number;
  status: BookStatus;
  rating?: number;
  startDate?: string;
  endDate?: string;
  createdDate: string;
}

export type BookListResponse = PaginatedListResponse<Book>;

export interface BookFormValues {
  title: string;
  author: string;
  coverUrl?: string;
  totalPages: number;
  currentPage: number;
  status: BookStatus;
  rating?: number;
  startDate?: string;
  endDate?: string;
}

export interface BookTableFilters {
  search?: string;
  pageIndex: number;
  pageSize: number;
  sort?: {
    field: string;
    dir: 'asc' | 'desc';
  };
  status?: BookStatus;
}

export const BookStatusLabels: Record<BookStatus, string> = {
  [BookStatus.ToRead]: 'Okunacak',
  [BookStatus.Reading]: 'Okunuyor',
  [BookStatus.Completed]: 'Tamamlandı',
  [BookStatus.Dropped]: 'Bırakıldı'
};


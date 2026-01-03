import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import { Book, BookFormValues, BookListResponse, BookTableFilters } from './types';

export async function fetchBooks(filters: BookTableFilters): Promise<BookListResponse> {
  const response = await api.post('/books/search', buildDataGridPayload(filters, 'Title'));
  return normalizePaginatedResponse<Book>(response.data);
}

export async function getBookById(id: string): Promise<Book> {
  const response = await api.get<ApiResult<Book>>(`/books/${id}`);
  const result = normalizeApiResult<Book>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Kitap bulunamadı');
  }
  return result.data;
}

export async function createBook(values: BookFormValues) {
  const response = await api.post<ApiResult>('/books', {
    Title: values.title,
    Author: values.author,
    CoverUrl: values.coverUrl || null,
    TotalPages: values.totalPages,
    CurrentPage: values.currentPage,
    Status: values.status,
    Rating: values.rating || null,
    StartDate: values.startDate || null,
    EndDate: values.endDate || null
  });
  return normalizeApiResult(response.data);
}

export async function updateBook(id: string, values: BookFormValues) {
  const response = await api.put<ApiResult>(`/books/${id}`, {
    Id: id,
    Title: values.title,
    Author: values.author,
    CoverUrl: values.coverUrl || null,
    TotalPages: values.totalPages,
    CurrentPage: values.currentPage,
    Status: values.status,
    Rating: values.rating || null,
    StartDate: values.startDate || null,
    EndDate: values.endDate || null
  });
  return normalizeApiResult(response.data);
}

export async function deleteBook(id: string) {
  const response = await api.delete<ApiResult>(`/books/${id}`);
  return normalizeApiResult(response.data);
}

export interface BookInfoFromIsbn {
  title: string;
  author: string;
  publisher?: string;
  publishedDate?: string;
  pageCount?: number;
  coverUrl?: string;
  description?: string;
  isbn?: string;
  language?: string;
  categories: string[];
}

export async function getBookByIsbn(isbn: string): Promise<BookInfoFromIsbn> {
  const response = await api.get<ApiResult<{ bookInfo: BookInfoFromIsbn }>>(`/books/isbn/${encodeURIComponent(isbn)}`);
  const result = normalizeApiResult<{ bookInfo: BookInfoFromIsbn }>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'ISBN ile kitap bulunamadı');
  }
  return result.data.bookInfo;
}


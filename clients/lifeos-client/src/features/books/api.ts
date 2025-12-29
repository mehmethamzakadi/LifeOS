import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import { Book, BookFormValues, BookListResponse, BookTableFilters } from './types';

export async function fetchBooks(filters: BookTableFilters): Promise<BookListResponse> {
  const response = await api.post('/book/search', buildDataGridPayload(filters, 'Title'));
  return normalizePaginatedResponse<Book>(response.data);
}

export async function getBookById(id: string): Promise<Book> {
  const response = await api.get<ApiResult<Book>>(`/book/${id}`);
  const result = normalizeApiResult<Book>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Kitap bulunamadı');
  }
  return result.data;
}

export async function createBook(values: BookFormValues) {
  const response = await api.post<ApiResult>('/book', {
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
  const response = await api.put<ApiResult>(`/book/${id}`, {
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
  const response = await api.delete<ApiResult>(`/book/${id}`);
  return normalizeApiResult(response.data);
}


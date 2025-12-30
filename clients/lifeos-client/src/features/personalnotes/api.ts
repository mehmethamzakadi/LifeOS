import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import { PersonalNote, PersonalNoteFormValues, PersonalNoteListResponse, PersonalNoteTableFilters } from './types';

export async function fetchPersonalNotes(filters: PersonalNoteTableFilters): Promise<PersonalNoteListResponse> {
  const response = await api.post('/personalnotes/search', buildDataGridPayload(filters, 'Title'));
  return normalizePaginatedResponse<PersonalNote>(response.data);
}

export async function getPersonalNoteById(id: string): Promise<PersonalNote> {
  const response = await api.get<ApiResult<PersonalNote>>(`/personalnotes/${id}`);
  const result = normalizeApiResult<PersonalNote>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Not bulunamadı');
  }
  return result.data;
}

export async function createPersonalNote(values: PersonalNoteFormValues) {
  const response = await api.post<ApiResult>('/personalnotes', {
    Title: values.title,
    Content: values.content,
    Category: values.category || null,
    IsPinned: values.isPinned,
    Tags: values.tags || null
  });
  return normalizeApiResult(response.data);
}

export async function updatePersonalNote(id: string, values: PersonalNoteFormValues) {
  const response = await api.put<ApiResult>(`/personalnotes/${id}`, {
    Id: id,
    Title: values.title,
    Content: values.content,
    Category: values.category || null,
    IsPinned: values.isPinned,
    Tags: values.tags || null
  });
  return normalizeApiResult(response.data);
}

export async function deletePersonalNote(id: string) {
  const response = await api.delete<ApiResult>(`/personalnotes/${id}`);
  return normalizeApiResult(response.data);
}


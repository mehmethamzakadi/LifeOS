import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import {
  Category,
  CategoryFormValues,
  CategoryListResponse,
  CategoryTableFilters
} from './types';

export async function fetchCategories(
  filters: CategoryTableFilters
): Promise<CategoryListResponse> {
  const response = await api.post('/categories/search', buildDataGridPayload(filters, 'Name'));
  return normalizePaginatedResponse<Category>(response.data);
}

export async function createCategory(values: CategoryFormValues) {
  const response = await api.post<ApiResult>('/categories', {
    Name: values.name,
    Description: values.description || null,
    ParentId: values.parentId || null
  });
  return normalizeApiResult(response.data);
}

export async function updateCategory(id: string, values: CategoryFormValues) {
  const response = await api.put<ApiResult>(`/categories/${id}`, {
    Id: id,
    Name: values.name,
    Description: values.description || null,
    ParentId: values.parentId || null
  });
  return normalizeApiResult(response.data);
}

export async function deleteCategory(id: string) {
  const response = await api.delete<ApiResult>(`/categories/${id}`);
  return normalizeApiResult(response.data);
}

export async function getAllCategories(): Promise<Category[]> {
  const response = await api.get('/categories');
  const data = response.data;
  
  // Backend'den direkt array geliyorsa
  if (Array.isArray(data)) {
    return data.map((item: any) => ({
      id: item.id || item.Id,
      name: item.name || item.Name,
      description: item.description || item.Description,
      parentId: item.parentId || item.ParentId,
      createdDate: item.createdDate || item.CreatedDate || new Date().toISOString()
    }));
  }
  
  // Eğer wrapper içindeyse
  if (data && Array.isArray(data.data)) {
    return data.data.map((item: any) => ({
      id: item.id || item.Id,
      name: item.name || item.Name,
      description: item.description || item.Description,
      parentId: item.parentId || item.ParentId,
      createdDate: item.createdDate || item.CreatedDate || new Date().toISOString()
    }));
  }
  
  // Boş array döndür
  return [];
}

export async function generateCategoryDescription(categoryName: string): Promise<string> {
  const response = await api.get<ApiResult<{ description: string }>>('/categories/generate-description', {
    params: { categoryName }
  });
  
  const result = normalizeApiResult<{ description: string }>(response.data);
  
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Açıklama üretilemedi');
  }
  
  return result.data.description;
}

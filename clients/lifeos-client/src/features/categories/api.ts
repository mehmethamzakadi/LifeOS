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
  const response = await api.post('/category/search', buildDataGridPayload(filters, 'Name'));
  return normalizePaginatedResponse<Category>(response.data);
}

export async function createCategory(values: CategoryFormValues) {
  const response = await api.post<ApiResult>('/category', {
    Name: values.name,
    Description: values.description || null,
    ParentId: values.parentId || null
  });
  return normalizeApiResult(response.data);
}

export async function updateCategory(id: string, values: CategoryFormValues) {
  const response = await api.put<ApiResult>(`/category/${id}`, {
    Id: id,
    Name: values.name,
    Description: values.description || null,
    ParentId: values.parentId || null
  });
  return normalizeApiResult(response.data);
}

export async function deleteCategory(id: string) {
  const response = await api.delete<ApiResult>(`/category/${id}`);
  return normalizeApiResult(response.data);
}

export async function getAllCategories(): Promise<Category[]> {
  const response = await api.get('/category');
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
  const response = await api.get<{ description: string }>('/category/generate-description', {
    params: { categoryName }
  });
  return response.data.description;
}

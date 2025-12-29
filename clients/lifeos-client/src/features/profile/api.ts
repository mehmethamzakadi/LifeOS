import api from '../../lib/axios';
import { ApiResult, normalizeApiResult } from '../../types/api';
import { UserProfile, UpdateProfileFormValues, ChangePasswordFormValues } from './types';

export async function getCurrentUserProfile(): Promise<UserProfile> {
  const response = await api.get<ApiResult<UserProfile>>('/profile');
  const result = normalizeApiResult<UserProfile>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Profil bilgileri alınamadı');
  }
  return result.data;
}

export async function updateProfile(data: UpdateProfileFormValues): Promise<ApiResult> {
  const response = await api.put('/profile', data);
  return normalizeApiResult(response.data);
}

export async function changePassword(data: ChangePasswordFormValues): Promise<ApiResult> {
  const response = await api.post('/profile/change-password', data);
  return normalizeApiResult(response.data);
}

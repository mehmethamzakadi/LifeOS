import api from '../../lib/axios';
import { ApiResult, normalizeApiResult, normalizePaginatedResponse } from '../../types/api';
import { buildDataGridPayload } from '../../lib/data-grid-helpers';
import { WalletTransaction, WalletTransactionFormValues, WalletTransactionListResponse, WalletTransactionTableFilters } from './types';

export async function fetchWalletTransactions(filters: WalletTransactionTableFilters): Promise<WalletTransactionListResponse> {
  const response = await api.post('/wallettransaction/search', buildDataGridPayload(filters, 'Title'));
  return normalizePaginatedResponse<WalletTransaction>(response.data);
}

export async function getWalletTransactionById(id: string): Promise<WalletTransaction> {
  const response = await api.get<ApiResult<WalletTransaction>>(`/wallettransaction/${id}`);
  const result = normalizeApiResult<WalletTransaction>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'İşlem bulunamadı');
  }
  return result.data;
}

export async function createWalletTransaction(values: WalletTransactionFormValues) {
  const response = await api.post<ApiResult>('/wallettransaction', {
    Title: values.title,
    Amount: values.amount,
    Type: values.type,
    Category: values.category,
    TransactionDate: values.transactionDate
  });
  return normalizeApiResult(response.data);
}

export async function updateWalletTransaction(id: string, values: WalletTransactionFormValues) {
  const response = await api.put<ApiResult>(`/wallettransaction/${id}`, {
    Id: id,
    Title: values.title,
    Amount: values.amount,
    Type: values.type,
    Category: values.category,
    TransactionDate: values.transactionDate
  });
  return normalizeApiResult(response.data);
}

export async function deleteWalletTransaction(id: string) {
  const response = await api.delete<ApiResult>(`/wallettransaction/${id}`);
  return normalizeApiResult(response.data);
}


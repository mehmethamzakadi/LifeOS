import { PaginatedListResponse } from '../../types/api';

export enum TransactionType {
  Income = 0,
  Expense = 1
}

export enum TransactionCategory {
  Salary = 0,
  Freelance = 1,
  Food = 2,
  Bills = 3,
  Entertainment = 4,
  Other = 5
}

export interface WalletTransaction {
  id: string;
  title: string;
  amount: number;
  type: TransactionType;
  category: TransactionCategory;
  transactionDate: string;
  createdDate: string;
}

export type WalletTransactionListResponse = PaginatedListResponse<WalletTransaction>;

export interface WalletTransactionFormValues {
  title: string;
  amount: number;
  type: TransactionType;
  category: TransactionCategory;
  transactionDate: string;
}

export interface WalletTransactionTableFilters {
  search?: string;
  pageIndex: number;
  pageSize: number;
  sort?: {
    field: string;
    dir: 'asc' | 'desc';
  };
  type?: TransactionType;
  category?: TransactionCategory;
}

export const TransactionTypeLabels: Record<TransactionType, string> = {
  [TransactionType.Income]: 'Gelir',
  [TransactionType.Expense]: 'Gider'
};

export const TransactionCategoryLabels: Record<TransactionCategory, string> = {
  [TransactionCategory.Salary]: 'Maaş',
  [TransactionCategory.Freelance]: 'Freelance',
  [TransactionCategory.Food]: 'Yemek',
  [TransactionCategory.Bills]: 'Faturalar',
  [TransactionCategory.Entertainment]: 'Eğlence',
  [TransactionCategory.Other]: 'Diğer'
};


import { PaginatedListResponse } from '../../types/api';

export interface PersonalNote {
  id: string;
  title: string;
  content: string;
  category?: string;
  isPinned: boolean;
  tags?: string;
  createdDate: string;
}

export type PersonalNoteListResponse = PaginatedListResponse<PersonalNote>;

export interface PersonalNoteFormValues {
  title: string;
  content: string;
  category?: string;
  isPinned: boolean;
  tags?: string;
}

export interface PersonalNoteTableFilters {
  search?: string;
  pageIndex: number;
  pageSize: number;
  sort?: {
    field: string;
    dir: 'asc' | 'desc';
  };
  category?: string;
  isPinned?: boolean;
}


import { PaginatedListResponse } from '../../types/api';

export enum GameStatus {
  Backlog = 0,
  Playing = 1,
  Completed = 2,
  Abandoned = 3
}

export interface Game {
  id: string;
  title: string;
  coverUrl?: string;
  gamePlatformId: string;
  gamePlatformName: string;
  gameStoreId: string;
  gameStoreName: string;
  status: GameStatus;
  isOwned: boolean;
  createdDate: string;
}

export type GameListResponse = PaginatedListResponse<Game>;

export interface GameFormValues {
  title: string;
  coverUrl?: string;
  gamePlatformId: string;
  gameStoreId: string;
  status: GameStatus;
  isOwned: boolean;
}

export interface GameTableFilters {
  search?: string;
  pageIndex: number;
  pageSize: number;
  sort?: {
    field: string;
    dir: 'asc' | 'desc';
  };
  gamePlatformId?: string;
  status?: GameStatus;
}

export const GameStatusLabels: Record<GameStatus, string> = {
  [GameStatus.Backlog]: 'Beklemede',
  [GameStatus.Playing]: 'Oynanıyor',
  [GameStatus.Completed]: 'Tamamlandı',
  [GameStatus.Abandoned]: 'Bırakıldı'
};


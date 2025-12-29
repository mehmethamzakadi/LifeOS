import { PaginatedListResponse } from '../../types/api';

export enum GamePlatform {
  PC = 0,
  PS5 = 1,
  Xbox = 2,
  Switch = 3,
  Mobile = 4
}

export enum GameStore {
  Steam = 0,
  Epic = 1,
  PS_Store = 2,
  Xbox_Store = 3,
  Physical = 4
}

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
  platform: GamePlatform;
  store: GameStore;
  status: GameStatus;
  isOwned: boolean;
  createdDate: string;
}

export type GameListResponse = PaginatedListResponse<Game>;

export interface GameFormValues {
  title: string;
  coverUrl?: string;
  platform: GamePlatform;
  store: GameStore;
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
  platform?: GamePlatform;
  status?: GameStatus;
}

export const GamePlatformLabels: Record<GamePlatform, string> = {
  [GamePlatform.PC]: 'PC',
  [GamePlatform.PS5]: 'PlayStation 5',
  [GamePlatform.Xbox]: 'Xbox',
  [GamePlatform.Switch]: 'Nintendo Switch',
  [GamePlatform.Mobile]: 'Mobil'
};

export const GameStoreLabels: Record<GameStore, string> = {
  [GameStore.Steam]: 'Steam',
  [GameStore.Epic]: 'Epic Games',
  [GameStore.PS_Store]: 'PlayStation Store',
  [GameStore.Xbox_Store]: 'Xbox Store',
  [GameStore.Physical]: 'Fiziksel'
};

export const GameStatusLabels: Record<GameStatus, string> = {
  [GameStatus.Backlog]: 'Beklemede',
  [GameStatus.Playing]: 'Oynanıyor',
  [GameStatus.Completed]: 'Tamamlandı',
  [GameStatus.Abandoned]: 'Bırakıldı'
};


import api from '../../lib/axios';
import { ApiResult, normalizeApiResult } from '../../types/api';
import {
  MusicConnectionStatus,
  MusicAuthorizationUrl,
  ConnectMusicResponse,
  CurrentlyPlayingTrack,
  SavedTrack,
  Playlist,
  ListeningStats,
  AnalyzeArtistResponse
} from './types';

export async function getConnectionStatus(): Promise<MusicConnectionStatus> {
  const response = await api.get<ApiResult<MusicConnectionStatus>>('/music/connection-status');
  const result = normalizeApiResult<MusicConnectionStatus>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Bağlantı durumu alınamadı');
  }
  return result.data;
}

export async function getAuthorizationUrl(): Promise<MusicAuthorizationUrl> {
  const response = await api.get<ApiResult<MusicAuthorizationUrl>>('/music/authorization-url');
  const result = normalizeApiResult<MusicAuthorizationUrl>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Authorization URL alınamadı');
  }
  return result.data;
}

export async function connectMusic(code: string, state: string): Promise<ConnectMusicResponse> {
  const response = await api.post<ApiResult<ConnectMusicResponse>>('/music/connect', {
    code,
    state
  });
  const result = normalizeApiResult<ConnectMusicResponse>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Spotify bağlantısı kurulamadı');
  }
  return result.data;
}

export async function disconnectMusic(): Promise<void> {
  const response = await api.post<ApiResult>('/music/disconnect');
  const result = normalizeApiResult(response.data);
  if (!result.success) {
    throw new Error(result.message || 'Spotify bağlantısı kesilemedi');
  }
}

export async function getCurrentTrack(): Promise<CurrentlyPlayingTrack | null> {
  const response = await api.get<ApiResult<CurrentlyPlayingTrack>>('/music/current-track');
  const result = normalizeApiResult<CurrentlyPlayingTrack>(response.data);
  if (!result.success) {
    return null;
  }
  return result.data || null;
}

export async function getSavedTracks(): Promise<SavedTrack[]> {
  const response = await api.get<ApiResult<{ tracks: SavedTrack[] }>>('/music/saved-tracks');
  const result = normalizeApiResult<{ tracks: SavedTrack[] }>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Kaydedilmiş şarkılar alınamadı');
  }
  return result.data.tracks;
}

export async function saveTrack(trackId: string, notes?: string): Promise<SavedTrack> {
  const response = await api.post<ApiResult<SavedTrack>>('/music/saved-tracks', {
    spotifyTrackId: trackId,
    notes
  });
  const result = normalizeApiResult<SavedTrack>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Şarkı kaydedilemedi');
  }
  return result.data;
}

export async function deleteSavedTrack(id: string): Promise<void> {
  const response = await api.delete<ApiResult>(`/music/saved-tracks/${id}`);
  const result = normalizeApiResult(response.data);
  if (!result.success) {
    throw new Error(result.message || 'Şarkı silinemedi');
  }
}

export async function getPlaylists(): Promise<Playlist[]> {
  const response = await api.get<ApiResult<Playlist[]>>('/music/playlists');
  const result = normalizeApiResult<Playlist[]>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || "Playlist'ler alınamadı");
  }
  return result.data;
}

export async function getListeningStats(period: 'daily' | 'weekly' | 'monthly' = 'weekly'): Promise<ListeningStats> {
  const response = await api.get<ApiResult<ListeningStats>>(`/music/stats?period=${period}`);
  const result = normalizeApiResult<ListeningStats>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'İstatistikler alınamadı');
  }
  return {
    topTracks: result.data.topTracks || [],
    topArtists: result.data.topArtists || [],
    totalListeningTime: result.data.totalListeningTime || 0,
    mostListenedGenre: result.data.mostListenedGenre
  };
}

export async function analyzeArtist(artistName: string): Promise<AnalyzeArtistResponse> {
  const response = await api.get<ApiResult<AnalyzeArtistResponse>>(`/music/analyze-artist?artistName=${encodeURIComponent(artistName)}`);
  const result = normalizeApiResult<AnalyzeArtistResponse>(response.data);
  if (!result.success || !result.data) {
    throw new Error(result.message || 'Sanatçı analizi yapılamadı');
  }
  return result.data;
}


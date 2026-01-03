export interface MusicConnectionStatus {
  isConnected: boolean;
  spotifyUserId?: string;
  spotifyUserName?: string;
  isTokenExpired?: boolean;
}

export interface MusicAuthorizationUrl {
  authorizationUrl: string;
  state: string;
}

export interface ConnectMusicResponse {
  isConnected: boolean;
  message: string;
}

export interface SpotifyTrack {
  id: string;
  name: string;
  artists: SpotifyArtist[];
  album?: SpotifyAlbum;
  durationMs: number;
  previewUrl?: string;
  externalUrl?: string;
}

export interface SpotifyArtist {
  id: string;
  name: string;
}

export interface SpotifyAlbum {
  id: string;
  name: string;
  images: SpotifyImage[];
}

export interface SpotifyImage {
  url: string;
  height?: number;
  width?: number;
}

export interface CurrentlyPlayingTrack {
  isPlaying: boolean;
  item?: SpotifyTrack;
  progressMs?: number;
  timestamp?: number;
}

export interface SavedTrack {
  id: string; // Guid as string
  userId: string; // Guid as string
  spotifyTrackId: string;
  name: string;
  artist: string;
  album?: string;
  albumCoverUrl?: string;
  durationMs?: number;
  savedAt?: string;
  notes?: string;
  createdDate: string;
}

export interface Playlist {
  id: string;
  name: string;
  description?: string;
  images: SpotifyImage[];
  owner?: {
    id: string;
    displayName: string;
  };
  tracksTotal: number;
}

export interface ListeningStats {
  topTracks: SpotifyTrack[];
  topArtists: SpotifyArtist[];
  totalListeningTime: number;
  mostListenedGenre?: string;
}


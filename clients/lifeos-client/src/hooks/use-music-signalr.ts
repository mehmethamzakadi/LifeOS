import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuth } from './use-auth';
import { CurrentlyPlayingTrack } from '../features/music/types';
import { queryClient } from '../providers/query-client';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:6060';

/**
 * Music SignalR bağlantısı için hook
 * Real-time current track güncellemelerini dinler
 */
export function useMusicSignalR(enabled: boolean = true) {
  const { token, isAuthenticated } = useAuth();
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [connectionError, setConnectionError] = useState<string | null>(null);

  useEffect(() => {
    if (!enabled || !isAuthenticated || !token) {
      // Bağlantıyı kapat
      if (connectionRef.current) {
        connectionRef.current.stop().catch(console.error);
        connectionRef.current = null;
        setIsConnected(false);
      }
      return;
    }

    // SignalR bağlantısı oluştur
    // SignalR hub URL'i /api prefix'i olmadan
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/music`, {
        accessTokenFactory: () => token,
        withCredentials: true,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s, sonra her 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .build();

    connectionRef.current = connection;

    // Bağlantı event'leri
    connection.onclose((error) => {
      setIsConnected(false);
      if (error) {
        setConnectionError('SignalR bağlantısı kesildi');
        console.error('SignalR connection closed:', error);
      }
    });

    connection.onreconnecting((error) => {
      setIsConnected(false);
      setConnectionError('Yeniden bağlanılıyor...');
      console.log('SignalR reconnecting:', error);
    });

    connection.onreconnected((connectionId) => {
      setIsConnected(true);
      setConnectionError(null);
      console.log('SignalR reconnected:', connectionId);
    });

    // Current track güncellemelerini dinle
    connection.on('CurrentTrackUpdated', (data: CurrentlyPlayingTrack | null) => {
      // React Query cache'ini güncelle
      queryClient.setQueryData(['music-current-track'], data);
    });

    // Bağlantıyı başlat
    connection
      .start()
      .then(() => {
        setIsConnected(true);
        setConnectionError(null);
        console.log('SignalR connected to MusicHub');
      })
      .catch((error) => {
        setIsConnected(false);
        setConnectionError('SignalR bağlantısı kurulamadı');
        console.error('SignalR connection error:', error);
      });

    // Cleanup
    return () => {
      connection.stop().catch(console.error);
      connectionRef.current = null;
    };
  }, [enabled, isAuthenticated, token]);

  return {
    isConnected,
    connectionError
  };
}


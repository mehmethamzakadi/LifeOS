import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Music, Link2, Unlink, Loader2, Play, Pause, Heart, BarChart3, Wifi, WifiOff, XCircle, TrendingUp } from 'lucide-react';
import { getConnectionStatus, getAuthorizationUrl, disconnectMusic, getCurrentTrack, getSavedTracks, getListeningStats, saveTrack, deleteSavedTrack, analyzeVibe } from '../../features/music/api';
import { useMusicSignalR } from '../../hooks/use-music-signalr';
import { Button } from '../../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../../components/ui/card';
import { Badge } from '../../components/ui/badge';
import toast from 'react-hot-toast';
import { handleApiError } from '../../lib/api-error';

export function MusicPage() {
  const queryClient = useQueryClient();
  const [selectedPeriod, setSelectedPeriod] = useState<'daily' | 'weekly' | 'monthly'>('weekly');
  const [vibeTimeRange, setVibeTimeRange] = useState<'short_term' | 'medium_term' | 'long_term'>('short_term');

  // Connection status
  const { data: connectionStatus, isLoading: isLoadingStatus } = useQuery({
    queryKey: ['music-connection-status'],
    queryFn: getConnectionStatus,
    retry: 1
  });

  // SignalR bağlantısı (real-time updates için)
  const { isConnected: isSignalRConnected } = useMusicSignalR(
    connectionStatus?.isConnected === true
  );

  // Currently playing track (SignalR ile real-time güncellenir, fallback olarak polling)
  const { data: currentTrack } = useQuery({
    queryKey: ['music-current-track'],
    queryFn: getCurrentTrack,
    enabled: connectionStatus?.isConnected === true,
    refetchInterval: isSignalRConnected ? false : 5000 // SignalR bağlıysa polling yapma
  });

  // Saved tracks
  const { data: savedTracks, isLoading: isLoadingSavedTracks } = useQuery({
    queryKey: ['music-saved-tracks'],
    queryFn: getSavedTracks,
    enabled: connectionStatus?.isConnected === true
  });

  // Check if current track is saved
  const isCurrentTrackSaved = currentTrack?.item?.id 
    ? savedTracks?.some(t => t.spotifyTrackId === currentTrack.item?.id)
    : false;

  // Save track mutation
  const saveTrackMutation = useMutation({
    mutationFn: (trackId: string) => saveTrack(trackId),
    onSuccess: () => {
      toast.success('Şarkı beğenilenlere eklendi');
      queryClient.invalidateQueries({ queryKey: ['music-saved-tracks'] });
    },
    onError: (error) => {
      handleApiError(error);
    }
  });

  // Delete saved track mutation
  const deleteSavedTrackMutation = useMutation({
    mutationFn: (trackId: string) => deleteSavedTrack(trackId),
    onSuccess: () => {
      toast.success('Şarkı beğenilenlerden çıkarıldı');
      queryClient.invalidateQueries({ queryKey: ['music-saved-tracks'] });
    },
    onError: (error) => {
      handleApiError(error);
    }
  });

  const handleToggleSaveTrack = () => {
    if (!currentTrack?.item?.id) return;
    
    const trackId = currentTrack.item.id;
    
    if (isCurrentTrackSaved) {
      const savedTrack = savedTracks?.find(t => t.spotifyTrackId === trackId);
      if (savedTrack) {
        deleteSavedTrackMutation.mutate(savedTrack.id);
      }
    } else {
      saveTrackMutation.mutate(trackId);
    }
  };


  // Listening stats
  const { data: stats, isLoading: isLoadingStats } = useQuery({
    queryKey: ['music-stats', selectedPeriod],
    queryFn: () => getListeningStats(selectedPeriod),
    enabled: connectionStatus?.isConnected === true
  });

  // Vibe analysis
  const { data: vibeAnalysis, isLoading: isLoadingVibe } = useQuery({
    queryKey: ['music-vibe', vibeTimeRange],
    queryFn: () => analyzeVibe(vibeTimeRange),
    enabled: connectionStatus?.isConnected === true,
    retry: 1
  });

  // Connect mutation
  const connectMutation = useMutation({
    mutationFn: async () => {
      const { authorizationUrl, state } = await getAuthorizationUrl();
      
      // Store state in both localStorage and sessionStorage for callback verification
      if (!state) {
        throw new Error('State parametresi alınamadı');
      }
      
      // State'i hem localStorage hem sessionStorage'a kaydet (redundancy)
      try {
        localStorage.setItem('spotify_oauth_state', state);
        sessionStorage.setItem('spotify_oauth_state', state);
        console.log('State kaydedildi (localStorage + sessionStorage):', state.substring(0, 20) + '...');
        
        // Doğrulama - her ikisini de kontrol et
        const storedLocal = localStorage.getItem('spotify_oauth_state');
        const storedSession = sessionStorage.getItem('spotify_oauth_state');
        
        if (storedLocal !== state && storedSession !== state) {
          console.error('State kaydedilemedi!', { storedLocal, storedSession, expected: state });
          throw new Error('State kaydedilemedi');
        }
        
        console.log('State başarıyla kaydedildi ve doğrulandı');
      } catch (error) {
        console.error('State kaydedilirken hata:', error);
        throw new Error('State kaydedilemedi: ' + (error instanceof Error ? error.message : String(error)));
      }
      
      // Yönlendirme
      window.location.href = authorizationUrl;
    },
    onError: (error) => {
      handleApiError(error);
    }
  });

  // Disconnect mutation
  const disconnectMutation = useMutation({
    mutationFn: disconnectMusic,
    onSuccess: () => {
      toast.success('Spotify bağlantısı kesildi');
      queryClient.invalidateQueries({ queryKey: ['music-connection-status'] });
      queryClient.invalidateQueries({ queryKey: ['music-current-track'] });
      queryClient.invalidateQueries({ queryKey: ['music-saved-tracks'] });
      queryClient.invalidateQueries({ queryKey: ['music-stats'] });
    },
    onError: (error) => {
      handleApiError(error);
    }
  });

  const handleConnect = () => {
    connectMutation.mutate();
  };

  const handleDisconnect = () => {
    if (confirm('Spotify bağlantısını kesmek istediğinize emin misiniz?')) {
      disconnectMutation.mutate();
    }
  };

  if (isLoadingStatus) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!connectionStatus?.isConnected) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Müzik</h1>
            <p className="text-muted-foreground">Spotify hesabınızı bağlayarak müzik dinleme istatistiklerinizi görüntüleyin</p>
          </div>
        </div>

        <Card>
          <CardContent className="pt-6">
            <div className="flex flex-col items-center justify-center py-12 space-y-4">
              <Music className="h-16 w-16 text-muted-foreground" />
              <div className="text-center space-y-2">
                <h3 className="text-xl font-semibold">Spotify Hesabı Bağlı Değil</h3>
                <p className="text-muted-foreground max-w-md">
                  Spotify hesabınızı bağlayarak dinleme geçmişinizi, beğendiğiniz şarkıları ve istatistiklerinizi görüntüleyebilirsiniz.
                </p>
              </div>
              <Button
                onClick={handleConnect}
                disabled={connectMutation.isPending}
                size="lg"
                className="mt-4"
              >
                {connectMutation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Yönlendiriliyor...
                  </>
                ) : (
                  <>
                    <Link2 className="mr-2 h-4 w-4" />
                    Spotify'a Bağlan
                  </>
                )}
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Müzik</h1>
          <div className="flex items-center gap-3 mt-1">
            {connectionStatus.spotifyUserName && (
              <p className="text-muted-foreground">
                Bağlı: {connectionStatus.spotifyUserName}
              </p>
            )}
            {connectionStatus?.isConnected && (
              <div className="flex items-center gap-2">
                {isSignalRConnected ? (
                  <Badge variant="outline" className="text-green-600 border-green-600">
                    <Wifi className="h-3 w-3 mr-1" />
                    Canlı
                  </Badge>
                ) : (
                  <Badge variant="outline" className="text-yellow-600 border-yellow-600">
                    <WifiOff className="h-3 w-3 mr-1" />
                    Yeniden bağlanıyor...
                  </Badge>
                )}
              </div>
            )}
          </div>
        </div>
        <Button
          variant="outline"
          onClick={handleDisconnect}
          disabled={disconnectMutation.isPending}
        >
          {disconnectMutation.isPending ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Kesiliyor...
            </>
          ) : (
            <>
              <Unlink className="mr-2 h-4 w-4" />
              Bağlantıyı Kes
            </>
          )}
        </Button>
      </div>

      {/* Currently Playing */}
      {currentTrack?.item && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Play className="h-5 w-5" />
              Şu An Dinleniyor
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-4">
              {currentTrack.item.album?.images?.[0] && (
                <img
                  src={currentTrack.item.album.images[0].url}
                  alt={currentTrack.item.album.name}
                  className="w-20 h-20 rounded-lg object-cover"
                />
              )}
              <div className="flex-1">
                <h3 className="font-semibold text-lg">{currentTrack.item.name}</h3>
                <p className="text-muted-foreground">
                  {currentTrack.item.artists.map(a => a.name).join(', ')}
                </p>
                {currentTrack.item.album && (
                  <p className="text-sm text-muted-foreground mt-1">{currentTrack.item.album.name}</p>
                )}
              </div>
              <div className="flex items-center gap-2">
                {currentTrack.isPlaying && (
                  <Badge variant="default" className="bg-green-500">
                    <Pause className="h-3 w-3 mr-1" />
                    Çalıyor
                  </Badge>
                )}
                <Button
                  variant={isCurrentTrackSaved ? "default" : "outline"}
                  size="sm"
                  onClick={handleToggleSaveTrack}
                  disabled={saveTrackMutation.isPending || deleteSavedTrackMutation.isPending}
                >
                  <Heart className={`h-4 w-4 mr-1 ${isCurrentTrackSaved ? 'fill-current' : ''}`} />
                  {isCurrentTrackSaved ? 'Beğenildi' : 'Beğen'}
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Vibe Check - Ruh Hali Analizi */}
      {connectionStatus?.isConnected && (
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5" />
              Ruh Hali Analizi
            </CardTitle>
            <CardDescription>
              Dinlediğiniz müziklere göre ruh halinizi analiz ediyoruz
              <div className="flex gap-2 mt-2">
                {(['short_term', 'medium_term', 'long_term'] as const).map((range) => (
                  <Button
                    key={range}
                    variant={vibeTimeRange === range ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => setVibeTimeRange(range)}
                    disabled={isLoadingVibe}
                  >
                    {range === 'short_term' ? 'Son 4 Hafta' : range === 'medium_term' ? 'Son 6 Ay' : 'Tüm Zamanlar'}
                  </Button>
                ))}
              </div>
            </CardDescription>
          </CardHeader>
          <CardContent>
            {isLoadingVibe ? (
              <div className="flex items-center justify-center py-8">
                <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
              </div>
            ) : vibeAnalysis ? (
              <div className="space-y-6">
                {/* Mood Title & Icon */}
                <div className="text-center py-4 bg-muted rounded-lg">
                  <div className="text-4xl mb-2">{vibeAnalysis.moodIcon}</div>
                  <h3 className="text-lg font-semibold">{vibeAnalysis.moodTitle}</h3>
                  {vibeAnalysis.topGenre && (
                    <p className="text-sm text-muted-foreground mt-1">
                      En çok dinlenen: {vibeAnalysis.topGenre}
                    </p>
                  )}
                  <p className="text-xs text-muted-foreground mt-2">
                    {vibeAnalysis.analyzedTracksCount} şarkı analiz edildi
                  </p>
                </div>

                {/* Progress Bars */}
                <div className="space-y-4">
                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">Enerji Seviyesi</span>
                      <span className="text-sm text-muted-foreground">{vibeAnalysis.energyLevel}%</span>
                    </div>
                    <div className="w-full bg-muted rounded-full h-3">
                      <div
                        className="bg-red-500 h-3 rounded-full transition-all duration-500"
                        style={{ width: `${vibeAnalysis.energyLevel}%` }}
                      />
                    </div>
                  </div>

                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">Mutluluk Seviyesi</span>
                      <span className="text-sm text-muted-foreground">{vibeAnalysis.happinessLevel}%</span>
                    </div>
                    <div className="w-full bg-muted rounded-full h-3">
                      <div
                        className="bg-yellow-500 h-3 rounded-full transition-all duration-500"
                        style={{ width: `${vibeAnalysis.happinessLevel}%` }}
                      />
                    </div>
                  </div>

                  <div>
                    <div className="flex justify-between items-center mb-2">
                      <span className="text-sm font-medium">Dans Edilebilirlik</span>
                      <span className="text-sm text-muted-foreground">{vibeAnalysis.danceabilityLevel}%</span>
                    </div>
                    <div className="w-full bg-muted rounded-full h-3">
                      <div
                        className="bg-purple-500 h-3 rounded-full transition-all duration-500"
                        style={{ width: `${vibeAnalysis.danceabilityLevel}%` }}
                      />
                    </div>
                  </div>
                </div>
              </div>
            ) : (
              <div className="text-center py-8 text-muted-foreground">
                <p>Analiz yapılamadı. Biraz daha müzik dinleyin.</p>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Stats */}
      {stats && (
        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <BarChart3 className="h-5 w-5" />
                İstatistikler
              </CardTitle>
              <CardDescription>
                <div className="flex gap-2 mt-2">
                  {(['daily', 'weekly', 'monthly'] as const).map((period) => (
                    <Button
                      key={period}
                      variant={selectedPeriod === period ? 'default' : 'outline'}
                      size="sm"
                      onClick={() => setSelectedPeriod(period)}
                    >
                      {period === 'daily' ? 'Günlük' : period === 'weekly' ? 'Haftalık' : 'Aylık'}
                    </Button>
                  ))}
                </div>
              </CardDescription>
            </CardHeader>
            <CardContent>
              {isLoadingStats ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              ) : (
                <div className="space-y-4">
                  {stats.topTracks && stats.topTracks.length > 0 && (
                    <div>
                      <h4 className="font-semibold mb-2">En Çok Dinlenen Şarkılar</h4>
                      <div className="space-y-2">
                        {stats.topTracks.slice(0, 5).map((track, index) => (
                          <div key={track.id} className="flex items-center gap-2 text-sm">
                            <span className="text-muted-foreground w-6">{index + 1}.</span>
                            <span className="flex-1">{track.name}</span>
                            <span className="text-muted-foreground text-xs">
                              {track.artists[0]?.name}
                            </span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                  {stats.topArtists && stats.topArtists.length > 0 && (
                    <div>
                      <h4 className="font-semibold mb-2">En Çok Dinlenen Sanatçılar</h4>
                      <div className="space-y-2">
                        {stats.topArtists.slice(0, 5).map((artist, index) => (
                          <div key={artist.id} className="flex items-center gap-2 text-sm">
                            <span className="text-muted-foreground w-6">{index + 1}.</span>
                            <span className="flex-1">{artist.name}</span>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              )}
            </CardContent>
          </Card>

          {/* Saved Tracks */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Heart className="h-5 w-5" />
                Beğenilen Şarkılar
              </CardTitle>
            </CardHeader>
            <CardContent>
              {isLoadingSavedTracks ? (
                <div className="flex items-center justify-center py-8">
                  <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                </div>
              ) : savedTracks && savedTracks.length > 0 ? (
                <div className="space-y-2">
                  {savedTracks.slice(0, 10).map((track) => (
                    <div key={track.id} className="flex items-center gap-2 text-sm group">
                      {track.albumCoverUrl && (
                        <img
                          src={track.albumCoverUrl}
                          alt={track.name}
                          className="w-10 h-10 rounded object-cover"
                        />
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="font-medium truncate">{track.name}</p>
                        <p className="text-xs text-muted-foreground truncate">{track.artist}</p>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="opacity-0 group-hover:opacity-100 transition-opacity"
                        onClick={() => deleteSavedTrackMutation.mutate(track.id)}
                        disabled={deleteSavedTrackMutation.isPending}
                      >
                        <XCircle className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                  ))}
                  {savedTracks.length > 10 && (
                    <p className="text-xs text-muted-foreground text-center pt-2">
                      +{savedTracks.length - 10} şarkı daha
                    </p>
                  )}
                </div>
              ) : (
                <div className="text-center py-8">
                  <p className="text-muted-foreground mb-2">Henüz beğenilen şarkı yok</p>
                  <p className="text-xs text-muted-foreground">
                    Dinlediğiniz şarkıları beğenerek buraya ekleyebilirsiniz
                  </p>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      )}

    </div>
  );
}


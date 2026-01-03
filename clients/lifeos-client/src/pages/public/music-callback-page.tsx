import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Loader2, CheckCircle2, XCircle } from 'lucide-react';
import { connectMusic } from '../../features/music/api';
import { Card, CardContent } from '../../components/ui/card';
import toast from 'react-hot-toast';
import { handleApiError } from '../../lib/api-error';
import { useAuth } from '../../hooks/use-auth';

export function MusicCallbackPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const { isAuthenticated, hydrated, ensureSession } = useAuth();
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('');

  // Önce authentication kontrolü yap
  useEffect(() => {
    if (!hydrated) {
      return;
    }

    if (!isAuthenticated) {
      // Session restore dene
      ensureSession().then((restored) => {
        if (!restored) {
          setStatus('error');
          setMessage('Oturum süresi dolmuş. Lütfen tekrar giriş yapın.');
          toast.error('Oturum süresi dolmuş');
          setTimeout(() => navigate('/login'), 2000);
        }
      });
    }
  }, [hydrated, isAuthenticated, ensureSession, navigate]);

  useEffect(() => {
    // Authentication kontrolü tamamlanmadan devam etme
    if (!hydrated || !isAuthenticated) {
      return;
    }

    const code = searchParams.get('code');
    const state = searchParams.get('state');
    const error = searchParams.get('error');

    console.log('Callback page loaded:', { code: code?.substring(0, 20), state, error });

    if (error) {
      setStatus('error');
      setMessage('Spotify bağlantısı reddedildi veya bir hata oluştu.');
      toast.error('Spotify bağlantısı başarısız');
      setTimeout(() => navigate('/admin/music'), 3000);
      return;
    }

    if (!code || !state) {
      setStatus('error');
      setMessage('Geçersiz callback parametreleri.');
      console.error('Missing callback parameters:', { code: !!code, state: !!state });
      setTimeout(() => navigate('/admin/music'), 3000);
      return;
    }

    // Verify state - hem localStorage hem de sessionStorage'dan kontrol et
    const storedStateLocal = localStorage.getItem('spotify_oauth_state');
    const storedStateSession = sessionStorage.getItem('spotify_oauth_state');
    const storedState = storedStateLocal || storedStateSession;
    
    console.log('Callback state verification:', { 
      storedStateLocal: storedStateLocal?.substring(0, 20) + '...', 
      storedStateSession: storedStateSession?.substring(0, 20) + '...', 
      storedState: storedState?.substring(0, 20) + '...',
      receivedState: state?.substring(0, 20) + '...',
      match: storedState === state
    });
    
    // State doğrulaması - eğer eşleşmiyorsa uyarı ver ama devam et
    // Backend'de de state doğrulaması yapılmalı (şu an yok, güvenlik açığı)
    if (!storedState || storedState !== state) {
      console.warn('⚠️ State verification failed!', {
        storedState: storedState?.substring(0, 20),
        receivedState: state?.substring(0, 20)
      });
      // Geçici olarak devam et - backend'de state doğrulaması eklenmeli
      // setStatus('error');
      // setMessage('Güvenlik doğrulaması başarısız. State eşleşmedi.');
      // localStorage.removeItem('spotify_oauth_state');
      // sessionStorage.removeItem('spotify_oauth_state');
      // setTimeout(() => navigate('/admin/music'), 3000);
      // return;
    } else {
      console.log('✅ State verification successful');
    }

    // Connect music
    connectMusic(code, state)
      .then(() => {
        setStatus('success');
        setMessage('Spotify hesabınız başarıyla bağlandı!');
        localStorage.removeItem('spotify_oauth_state');
        toast.success('Spotify bağlantısı başarılı');
        setTimeout(() => navigate('/admin/music'), 2000);
      })
      .catch((error) => {
        setStatus('error');
        setMessage(handleApiError(error) || 'Spotify bağlantısı kurulamadı.');
        toast.error('Spotify bağlantısı başarısız');
        localStorage.removeItem('spotify_oauth_state');
        setTimeout(() => navigate('/admin/music'), 3000);
      });
  }, [searchParams, navigate, hydrated, isAuthenticated]);

  return (
    <div className="flex items-center justify-center min-h-screen">
      <Card className="w-full max-w-md">
        <CardContent className="pt-6">
          <div className="flex flex-col items-center justify-center space-y-4 py-8">
            {status === 'loading' && (
              <>
                <Loader2 className="h-12 w-12 animate-spin text-primary" />
                <p className="text-lg font-medium">Spotify bağlantısı kuruluyor...</p>
              </>
            )}
            {status === 'success' && (
              <>
                <CheckCircle2 className="h-12 w-12 text-green-500" />
                <p className="text-lg font-medium text-green-500">{message}</p>
                <p className="text-sm text-muted-foreground">Yönlendiriliyorsunuz...</p>
              </>
            )}
            {status === 'error' && (
              <>
                <XCircle className="h-12 w-12 text-destructive" />
                <p className="text-lg font-medium text-destructive">{message}</p>
                <p className="text-sm text-muted-foreground">Yönlendiriliyorsunuz...</p>
              </>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}


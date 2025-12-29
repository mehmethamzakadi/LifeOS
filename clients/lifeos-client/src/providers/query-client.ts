import { QueryClient } from '@tanstack/react-query';

/**
 * React Query Client yapılandırması
 * 
 * Global staleTime: 5 dakika (300 saniye)
 * - Çoğu veri için makul bir süre
 * - Gereksiz network trafiğini önler
 * - Sık değişen veriler için component bazında override kullanılabilir
 * 
 * Örnek override kullanımı:
 * ```tsx
 * const { data } = useQuery({
 *   queryKey: ['dashboard'],
 *   queryFn: fetchDashboard,
 *   staleTime: 30 * 1000, // Dashboard için 30 saniye (daha sık güncelleme)
 * });
 * 
 * const { data } = useQuery({
 *   queryKey: ['notifications'],
 *   queryFn: fetchNotifications,
 *   staleTime: 10 * 1000, // Bildirimler için 10 saniye (çok sık güncelleme)
 * });
 * ```
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      // ✅ Global staleTime: 5 dakika (300 saniye)
      // Bu, tüm sorgular için varsayılan değerdir
      // Sık değişen veriler (Dashboard, Bildirimler) için component bazında override kullanılmalıdır
      staleTime: 5 * 60 * 1000, // 5 dakika (önceden 5 saniye idi - çok agresifti)
      // Cache 10 dakika boyunca tutulur (garbage collection)
      gcTime: 10 * 60 * 1000, // 10 dakika (önceden 5 dakika idi)
      // Pencere focus olduğunda otomatik refetch YOK (manuel invalidation kullanıyoruz)
      refetchOnWindowFocus: false,
      // Mount olduğunda stale data'yı refetch et
      refetchOnMount: true,
      // Network yeniden bağlandığında refetch YOK
      refetchOnReconnect: false
    }
  }
});

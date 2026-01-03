import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const isProduction = mode === 'production';
  
  return {
    plugins: [react()],
    server: {
      port: 5173,
      host: '0.0.0.0', // Docker container'dan erişim için
      watch: {
        // Docker volume mount için polling kullan
        usePolling: true,
        interval: 1000
      },
      hmr: {
        // Docker container içinde HMR için client port
        // Tarayıcı host'tan bağlanacağı için localhost kullanılır
        clientPort: 5173
      }
    },
    build: {
      // Production build optimizasyonları
      minify: isProduction ? 'esbuild' : false,
      sourcemap: !isProduction,
      rollupOptions: {
        output: {
          // Code splitting için manual chunks - daha iyi performans için
          manualChunks: (id) => {
            // Node modules için vendor chunks
            if (id.includes('node_modules')) {
              // React core
              if (id.includes('react') || id.includes('react-dom') || id.includes('react-router')) {
                return 'react-vendor';
              }
              // UI libraries
              if (id.includes('@radix-ui') || id.includes('lucide-react') || id.includes('framer-motion')) {
                return 'ui-vendor';
              }
              // Data fetching & state
              if (id.includes('@tanstack/react-query') || id.includes('@tanstack/react-table') || id.includes('zustand')) {
                return 'query-vendor';
              }
              // Forms
              if (id.includes('react-hook-form') || id.includes('@hookform') || id.includes('zod')) {
                return 'form-vendor';
              }
              // SignalR
              if (id.includes('@microsoft/signalr')) {
                return 'signalr-vendor';
              }
              // Charts
              if (id.includes('recharts')) {
                return 'charts-vendor';
              }
              // QR/Barcode
              if (id.includes('@ericblade/quagga2') || id.includes('@zxing') || id.includes('html5-qrcode')) {
                return 'scanner-vendor';
              }
              // Date utilities
              if (id.includes('date-fns')) {
                return 'date-vendor';
              }
              // Axios
              if (id.includes('axios')) {
                return 'http-vendor';
              }
              // Diğer vendor paketleri
              return 'vendor';
            }
          },
          // Chunk dosya isimlendirme
          chunkFileNames: 'assets/[name]-[hash].js',
          entryFileNames: 'assets/[name]-[hash].js',
          assetFileNames: 'assets/[name]-[hash].[ext]'
        }
      },
      // Chunk size uyarısı için limit (1.5MB - daha büyük uygulamalar için normal)
      chunkSizeWarningLimit: 1500
    },
    // Environment variables için prefix
    envPrefix: 'VITE_'
  };
});

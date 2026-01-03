import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const isProduction = mode === 'production';
  
  return {
    plugins: [react()],
    // Build sırasında zararsız uyarıları bastır
    logLevel: isProduction ? 'warn' : 'info',
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
        // Rollup uyarılarını filtrele - SignalR paketindeki zararsız uyarıları bastır
        onwarn(warning, warn) {
          // SignalR paketindeki PURE comment uyarılarını bastır (zararsız - Rollup plugin uyarısı)
          if (
            warning.message?.includes('/*#__PURE__*/') &&
            warning.message?.includes('@microsoft/signalr')
          ) {
            return; // Bu uyarıyı gösterme
          }
          // Diğer uyarıları göster
          warn(warning);
        },
        output: {
          // Code splitting için manual chunks - React'in bütünlüğünü koruyarak
          manualChunks: {
            // React core - TÜM React paketleri birlikte olmalı
            'react-vendor': ['react', 'react-dom', 'react-router-dom'],
            // UI libraries
            'ui-vendor': [
              '@radix-ui/react-dialog',
              '@radix-ui/react-label',
              '@radix-ui/react-checkbox',
              '@radix-ui/react-slot',
              'lucide-react',
              'framer-motion'
            ],
            // Data fetching & state
            'query-vendor': [
              '@tanstack/react-query',
              '@tanstack/react-table',
              'zustand'
            ],
            // Forms
            'form-vendor': [
              'react-hook-form',
              '@hookform/resolvers',
              'zod'
            ],
            // SignalR
            'signalr-vendor': ['@microsoft/signalr'],
            // Charts
            'charts-vendor': ['recharts'],
            // HTTP client
            'http-vendor': ['axios']
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

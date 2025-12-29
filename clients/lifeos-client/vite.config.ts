import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const isProduction = mode === 'production';
  
  return {
    plugins: [react()],
    server: {
      port: 5173,
      host: '0.0.0.0'
    },
    build: {
      // Production build optimizasyonları
      minify: isProduction ? 'esbuild' : false,
      sourcemap: !isProduction,
      rollupOptions: {
        output: {
          // Code splitting için manual chunks
          manualChunks: {
            'react-vendor': ['react', 'react-dom', 'react-router-dom'],
            'ui-vendor': ['@radix-ui/react-dialog', '@radix-ui/react-label', '@radix-ui/react-checkbox'],
            'query-vendor': ['@tanstack/react-query', '@tanstack/react-table'],
            'form-vendor': ['react-hook-form', '@hookform/resolvers', 'zod']
          }
        }
      },
      // Chunk size uyarısı için limit
      chunkSizeWarningLimit: 1000
    },
    // Environment variables için prefix
    envPrefix: 'VITE_'
  };
});

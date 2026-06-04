import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5280', changeOrigin: true },
      '/hubs': { target: 'http://localhost:5280', ws: true, changeOrigin: true },
    },
  },
});

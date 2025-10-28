import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5188',
        changeOrigin: true
      },
      '/hubs': {
        target: 'http://localhost:5188',
        ws: true
      }
    }
  }
})

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
    plugins: [react()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
            '@components': path.resolve(__dirname, './src/components'),
            '@pages': path.resolve(__dirname, './src/pages'),
            '@services': path.resolve(__dirname, './src/service'),
            '@types': path.resolve(__dirname, './src/types'),
            '@utils': path.resolve(__dirname, './src/utils'),
            '@context': path.resolve(__dirname, './src/context'),
        },
    },
    server: {
        port: 5173,
        open: true,
        middlewareMode: false,
    },
    publicDir: 'public',
    build: {
        outDir: 'dist',
        sourcemap: false,
        minify: 'terser',
    }
})
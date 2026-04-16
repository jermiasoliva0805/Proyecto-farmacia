import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

/**
 * En Azure: Dos App Services separados, necesita URL completa del backend
 * En desarrollo: Usa URL configurada o localhost
 */
const getApiBaseUrl = () => {
    const isDev = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1';
    
    if (isDev) {
        return import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
    }
    
    // En Azure: URL completa del backend (App Service separado)
    // IMPORTANTE: Esto puede fallar por CORS si no está configurado en el backend
    return 'https://farmaciaapi-fghpeqfzgjgqcxdq.chilecentral-01.azurewebsites.net/api';
};

const API_BASE_URL = getApiBaseUrl();
console.log('🔌 API Base URL:', API_BASE_URL);

export const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Interceptor para agregar el token JWT a cada request
api.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem('farmacia_token');
        
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// Interceptor para manejar la expiración del token (Error 401)
api.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('farmacia_token');
            localStorage.removeItem('farmacia_user');
            
            if (!window.location.pathname.includes('/login')) {
                window.location.href = '/login';
            }
        }
        return Promise.reject(error);
    }
);
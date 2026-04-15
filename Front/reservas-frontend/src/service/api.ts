import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';

/**
 * IMPORTANTE: 
 * En producción (Azure): Usa rutas relativas /api/...
 * En desarrollo: Usa localhost o la variable VITE_API_BASE_URL
 * 
 * staticwebapp.config.json se encarga de la redirección real en Azure Static Web App
 */
const getApiBaseUrl = () => {
    // En development, usar la variable configurada
    if (import.meta.env.VITE_API_BASE_URL) {
        return import.meta.env.VITE_API_BASE_URL;
    }
    
    // En producción (Azure Static Web App), staticwebapp.config.json redirige /api al backend
    // Por eso usamos ruta relativa
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        return 'http://localhost:5000/api';
    }
    
    // Ruta relativa para Azure (staticwebapp.config.json la redirige)
    return '/api';
};

const API_BASE_URL = getApiBaseUrl();

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
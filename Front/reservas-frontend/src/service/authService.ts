import { api } from './api';
import { LoginDTO, AuthResponse, UserDTO } from '../types/auth.types';

export const authService = {
    
    async login(credentials: LoginDTO): Promise<AuthResponse> {
        try {
            // CAMBIO CLAVE: '/Auth/login' con A mayúscula para coincidir con el controlador .NET
            const response = await api.post<AuthResponse>('/Auth/login', credentials);
            
            // IMPORTANTE: Si la respuesta es exitosa, guardamos los datos
            if (response.data && response.data.token) {
                this.saveAuth(response.data.token, response.data.user);
            }
            
            return response.data;
        } catch (error: any) {
            console.error('Error en login:', error);
            const message = error.response?.data?.message || 'Error de conexión';
            throw new Error(message);
        }
    },

    saveAuth(token: string, user: UserDTO): void {
        localStorage.setItem('farmacia_token', token);
        localStorage.setItem('farmacia_user', JSON.stringify(user));
    },

    getStoredAuth(): { token: string | null; user: UserDTO | null } {
        const token = localStorage.getItem('farmacia_token');
        const userStr = localStorage.getItem('farmacia_user');
        const user = userStr ? JSON.parse(userStr) : null;
        return { token, user };
    },

    clearAuth(): void {
        localStorage.removeItem('farmacia_token');
        localStorage.removeItem('farmacia_user');
    },

    isAuthenticated(): boolean {
        return !!localStorage.getItem('farmacia_token');
    },

    async logout(): Promise<void> {
        this.clearAuth();
        window.location.href = '/login';
    }
};
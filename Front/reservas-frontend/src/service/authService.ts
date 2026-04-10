import axios from 'axios';
import { api } from "../service/api";
import { LoginDTO, AuthResponse, UserDTO } from '../types/auth.types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';
const API_URL = `${API_BASE_URL}/auth`;

export const authService = {

    async login(credentials: LoginDTO): Promise<AuthResponse> {
        try {
            const response = await api.post<AuthResponse>('/auth/login', credentials);
            return response.data;
        } catch (error) {
            console.error('Error en login:', error);
            throw error;
        }
    },

    async generateToken(user: UserDTO): Promise<string | null> {
        try {
            const response = await axios.post<{ token: string }>(`${API_URL}/token`, user);
            return response.data.token;
        } catch (error) {
            console. error('Error generando token:', error);
            return null;
        }
    },

    saveAuth(token: string, user:  any): void {
        localStorage. setItem('farmacia_token', token);
        localStorage.setItem('farmacia_user', JSON. stringify(user));
    },

    getStoredAuth(): { token: string | null; user: any | null } {
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
        const { token } = this.getStoredAuth();
        return !!token;
    },

    async logout(): Promise<void> {
        try {
            await api.post('/auth/logout');
        } catch (error) {
            console. error('Error en logout:', error);
        } finally {
            this.clearAuth();
        }
    },
}
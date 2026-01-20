import api from './api';
import { LoginDTO, AuthResponse } from '../types/auth.types';

export const authService = {
  async login(credentials: LoginDTO): Promise<AuthResponse> {
    // ESTO MOSTRARÁ EL JSON EXACTO EN LA CONSOLA
    console.log("--- DATOS QUE ESTÁS ENVIANDO ---");
    console.table(credentials); 
    
    try {
      // Forzamos el objeto para que coincida exactamente con lo que Swagger espera
      const payload = {
        usuario: credentials.usuario.trim(),
        password: credentials.password.trim()
      };

      console.log("JSON final enviado al servidor:", JSON.stringify(payload));

      // Importante: Verifica que la URL base en api.ts sea https://localhost:7075/api
      const response = await api.post<AuthResponse>('/Auth/login', payload);

      if (response.data && response.data.token) {
        localStorage.setItem('farmacia_token', response.data.token);
        localStorage.setItem('farmacia_user', JSON.stringify(response.data.user));
      }

      return response.data;
    } catch (error: any) {
      console.error("--- ERROR EN LA PETICIÓN ---");
      if (error.response) {
        // El servidor respondió con 400, 401, 500, etc.
        console.error("Código de error:", error.response.status);
        console.error("Respuesta del servidor:", error.response.data);
      } else if (error.request) {
        // La petición se hizo pero no hubo respuesta (CORS o Servidor apagado)
        console.error("No se recibió respuesta del servidor. Revisa si el Backend está corriendo en https://localhost:7075");
      } else {
        console.error("Error de configuración:", error.message);
      }
      throw error;
    }
  },

  getStoredAuth() {
    const token = localStorage.getItem('farmacia_token');
    const user = localStorage.getItem('farmacia_user');
    return {
      token,
      user: user ? JSON.parse(user) : null
    };
  },

  clearAuth() {
    localStorage.removeItem('farmacia_token');
    localStorage.removeItem('farmacia_user');
  }
};
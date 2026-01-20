import React, { createContext, useContext, useState, useEffect } from 'react';
import { authService } from '../service/authService'; // 👈 Importamos el servicio corregido
import { UserDTO } from '../types/auth.types';

interface AuthContextType {
  user: UserDTO | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (usuario: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  // ✅ Al montar, usamos el servicio para recuperar la sesión
  useEffect(() => {
    const { token, user: userData } = authService.getStoredAuth();

    if (token && userData) {
      setUser(userData);
      setIsAuthenticated(true);
    }
    setIsLoading(false);
  }, []);

  // ✅ Función de login corregida
  const login = async (usuario: string, password: string) => {
    try {
      setIsLoading(true);
      // 👈 Usamos el servicio que ya tiene la URL de https://localhost:7075 y /Auth/login
      const data = await authService.login({ usuario, password });

      setUser(data.user);
      setIsAuthenticated(true);
    } catch (error: any) {
      // Manejo de errores más limpio
      const errorMsg = error.response?.data?.message || 'Error al iniciar sesión';
      throw errorMsg;
    } finally {
      setIsLoading(false);
    }
  };

  // ✅ Función de logout usando el servicio
  const logout = () => {
    authService.clearAuth();
    setUser(null);
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider value={{ user, isAuthenticated, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth debe usarse dentro de AuthProvider');
  }
  return context;
};
export interface LoginDTO {
    usuario: string;
    password: string;
}

export interface UserDTO {
    id: number;
    usuario: string;
    nombreCompleto: string;
    email: string;
    rol: 'Encargado' | 'Operario' | 'Cadete';
    nombreSucursal: string;
}

export interface AuthResponse {
    token: string;
    user: UserDTO;
}

export interface AuthContextType {
    user: UserDTO | null;
    token: string | null;
    login: (credentials: LoginDTO) => Promise<void>;
    logout: () => void;
    isAuthenticated: boolean;
    isLoading: boolean;
}
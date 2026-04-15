import { api } from './api';
import { UserDTO } from '../types/auth.types.ts';

export interface RegisterDTO {
  nombre: string;
  apellido: string;
  usuarioNombre: string;
  contraseña: string;
  rol: string;
  mail: string;
  idSucursal: number;
  zonaId?: number | null; // Opcional: solo para cadetes
}

export interface UpdateUserDTO {
  nombre?: string;
  apellido?: string;
  contraseña?: string;
  rol?: string;
  mail?: string;
  idSucursal?: number;
  zonaId?: number | null; // Opcional: solo para cadetes
}

export const usuariosService = {
  // --- MÉTODOS DE LECTURA CON MAPEADO ---
  
  async getAllUsuarios(): Promise<UserDTO[]> {
    const response = await api.get<any[]>('/usuarios');
    
    return response.data.map(u => ({
      id: u.idUsuario, 
      usuario: u.usuario || u.usuarioNombre,
      nombreCompleto: u.nombreCompleto,
      email: u.email || u.mail,
      rol: u.rol,
      nombreSucursal: u.nombreSucursal
    }));
  },

  async getUsuarioById(id: number): Promise<UserDTO> {
    const response = await api.get<any>(`/usuarios/${id}`);
    const u = response.data;
    
    return {
      id: u.idUsuario,
      usuario: u.usuario || u.usuarioNombre,
      nombreCompleto: u.nombreCompleto,
      email: u.email || u.mail,
      rol: u.rol,
      nombreSucursal: u.nombreSucursal
    };
  },

  // --- MÉTODOS DE ESCRITURA PARA EL ABM (Nombres exactos de C#) ---

  async createUsuario(data: RegisterDTO): Promise<UserDTO> {
    const payload = {
      Nombre: data.nombre,
      Apellido: data.apellido,
      UsuarioNombre: data.usuarioNombre,
      Contraseña: data.contraseña,
      Rol: data.rol,
      Mail: data.mail,
      IDSucursal: data.idSucursal
    };

    const response = await api.post<any>('/usuarios', payload);
    const u = response.data;
    
    return {
      id: u.idUsuario,
      usuario: u.usuario || u.usuarioNombre,
      nombreCompleto: u.nombreCompleto,
      email: u.email || u.mail,
      rol: u.rol,
      nombreSucursal: u.nombreSucursal
    };
  },

  async updateUsuario(id: number, data: UpdateUserDTO): Promise<UserDTO> {
    const payload: any = {};
    
    if (data.nombre !== undefined) payload.Nombre = data.nombre;
    if (data.apellido !== undefined) payload.Apellido = data.apellido;
    if (data.contraseña !== undefined) payload.Contraseña = data.contraseña;
    if (data.rol !== undefined) payload.Rol = data.rol;
    if (data.mail !== undefined) payload.Mail = data.mail;
    if (data.idSucursal !== undefined) payload.IDSucursal = data.idSucursal;
    
    await api.put(`/usuarios/${id}`, payload);
    
    // Retornar los datos actualizados
    return this.getUsuarioById(id);
  },

  async deleteUsuario(id: number): Promise<void> {
    // El id que pasamos aquí es el u.id (que el service tradujo de idUsuario)
    await api.delete(`/usuarios/${id}`);
  },

  // --- MÉTODOS DE FILTRADO ---

  async getUsuariosByRol(rol: 'Encargado' | 'Operario' | 'Cadete'): Promise<UserDTO[]> {
    const usuarios = await this.getAllUsuarios();
    return usuarios.filter(u => u.rol === rol);
  },
};
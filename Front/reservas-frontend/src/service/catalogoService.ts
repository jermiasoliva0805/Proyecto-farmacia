import { api } from './api';
import { ClientDTO, ProductDTO, LocalidadDTO } from '../types/common.types';

export interface SucursalDTO {
    id: number;
    nombre: string;
    direccion: string;
    telefono: string;
}

export const catalogoService = {
    async getClientes(): Promise<ClientDTO[]> {
        try {
            console.log('📡 GET /clientes');
            const response = await api.get<ClientDTO[]>('/clientes');
            console.log('✅ Respuesta clientes:', response.data);
            return response.data;
        } catch (error) {
            console.error('❌ Error en getClientes:', error);
            throw error;
        }
    },

    async getProductos(): Promise<ProductDTO[]> {
        try {
            console.log('📡 GET /productos');
            const response = await api.get<ProductDTO[]>('/productos');
            console.log('✅ Respuesta productos:', response.data);
            return response.data;
        } catch (error) {
            console.error('❌ Error en getProductos:', error);
            throw error;
        }
    },

    async getLocalidades(): Promise<LocalidadDTO[]> {
        try {
            console.log('📡 GET /localidades');
            const response = await api.get<LocalidadDTO[]>('/localidades');
            console.log('✅ Respuesta localidades:', response.data);
            return response.data;
        } catch (error) {
            console.error('❌ Error en getLocalidades:', error);
            throw error;
        }
    },

    async getSucursales(): Promise<SucursalDTO[]> {
        try {
            console.log('📡 GET /sucursales');
            const response = await api.get<SucursalDTO[]>('/sucursales');
            console.log('✅ Respuesta sucursales:', response.data);
            return response.data;
        } catch (error) {
            console.error('❌ Error en getSucursales:', error);
            throw error;
        }
    },
};
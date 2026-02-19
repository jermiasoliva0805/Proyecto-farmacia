import { api } from './api'; 
import { RankingClienteDTO } from '../types/reporte.types';

export const getRankingClientes = async (): Promise<RankingClienteDTO[]> => {
    try {
        const response = await api.get('/reporte/ranking-clientes');
        return response.data;
    } catch (error) {
        console.error("Error en getRankingClientes:", error);
        throw error;
    }
};
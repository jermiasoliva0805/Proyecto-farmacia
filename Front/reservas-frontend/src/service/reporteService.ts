import { api } from './api'; 
// Importamos tu DTO desde el archivo donde están los otros
import { RankingClienteDTO } from '../types/pedido.types'; 

export const getRankingClientes = async (): Promise<RankingClienteDTO[]> => {
    try {
        const response = await api.get<RankingClienteDTO[]>('/reporte/ranking-clientes');
        return response.data;
    } catch (error) {
        console.error("Error al obtener el ranking:", error);
        throw error;
    }
};
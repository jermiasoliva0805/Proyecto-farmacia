import { api } from './api'; 
// Importamos tu DTO desde el archivo donde están los otros
import { ClienteFacturacionDTO, RankingClienteDTO } from '../types/pedido.types'; 

export const getRankingClientes = async (): Promise<RankingClienteDTO[]> => {
    try {
        const response = await api.get<RankingClienteDTO[]>('/reporte/ranking-clientes');
        return response.data;
    } catch (error) {
        console.error("Error al obtener el ranking:", error);
        throw error;
    }
};

export const getRankingClientesFacturacion = async (periodo: string, sucursal: string): Promise<ClienteFacturacionDTO[]> => {
    try {
        // 'api' es tu instancia de axios
        const response = await api.get<ClienteFacturacionDTO[]>('/reporte/clientes-facturacion', {
            params: { 
                dias: periodo, 
                // Si es 'todas' mandamos undefined para que no filtre
                sucursal: sucursal === 'todas' ? undefined : sucursal 
            }
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener ranking de facturación", error);
        throw error;
    }
};
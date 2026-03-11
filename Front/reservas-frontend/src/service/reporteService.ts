import { api } from './api'; 
// Importamos tu DTO desde el archivo donde están los otros
import { ClienteFacturacionDTO, RankingClienteDTO, TiemposProcesoDTO } from '../types/pedido.types'; 

export const getRankingClientes = async (periodo: string, idSucursal: number | null): Promise<RankingClienteDTO[]> => {
    try {
        const params: any = {
            dias: parseInt(periodo)
        };
        
        // Solo agregar idSucursal si es válido
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }
        
        const response = await api.get<RankingClienteDTO[]>('/reporte/ranking-clientes', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener el ranking:", error);
        throw error;
    }
};

export const getRankingClientesFacturacion = async (periodo: string, idSucursal: number | null): Promise<ClienteFacturacionDTO[]> => {
    try {
        // 'api' es tu instancia de axios
        const params: any = {
            dias: parseInt(periodo)
        };
        
        // Solo agregar idSucursal si es válido
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }
        
        const response = await api.get<ClienteFacturacionDTO[]>('/reporte/clientes-facturacion', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener ranking de facturación", error);
        throw error;
    }
};

export const getReporteTiempos = async (periodo: string, idSucursal: number | null): Promise<TiemposProcesoDTO> => {
    try {
        const params: any = {
            dias: parseInt(periodo)
        };
        
        // Solo agregar idSucursal si es válido
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }
        
        const response = await api.get<TiemposProcesoDTO>('/reporte/tiempos-proceso', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener reporte de tiempos:", error);
        throw error;
    }
};
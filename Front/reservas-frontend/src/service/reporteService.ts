import { api } from './api'; 
// Importamos tu DTO desde el archivo donde están los otros
import {
    ClienteFacturacionDTO,
    RankingClienteDTO,
    ReportePedidosCanceladosDTO,
    ReporteCancelacionesPorMotivoDTO,
    TopProductosDTO,
    TiemposProcesoDTO,
    ReporteFormasPagoDTO,
    ReporteEncuestaSatisfaccionDTO,
    PedidosPorZonaDTO
} from '../types/pedido.types';

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

export const getPedidosCancelados = async (
    fechaDesde?: string,
    fechaHasta?: string,
    idSucursal: number | null = null
): Promise<ReportePedidosCanceladosDTO> => {
    try {
        const params: any = {};
        
        if (fechaDesde) {
            params.fechaDesde = fechaDesde;
        }
        
        if (fechaHasta) {
            params.fechaHasta = fechaHasta;
        }
        
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }
        
        const response = await api.get<ReportePedidosCanceladosDTO>('/reporte/pedidos-cancelados', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener reporte de pedidos cancelados", error);
        throw error;
    }
};

export const getCancelacionesPorMotivo = async (
    fechaDesde?: string,
    fechaHasta?: string,
    idSucursal: number | null = null
): Promise<ReporteCancelacionesPorMotivoDTO> => {
    try {
        const params: any = {};
        
        if (fechaDesde) {
            params.fechaDesde = fechaDesde;
        }
        
        if (fechaHasta) {
            params.fechaHasta = fechaHasta;
        }
        
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }
        
        const response = await api.get<ReporteCancelacionesPorMotivoDTO>('/reporte/cancelaciones-por-motivo', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener reporte de cancelaciones por motivo", error);
        throw error;
    }
};

export const getTop10Productos = async (periodo: string, idSucursal: number | null): Promise<TopProductosDTO[]> => {
    try {
        const params: any = {
            dias: parseInt(periodo)
        };
        
        // Solo agregar idSucursal si es válido
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }
        
        const response = await api.get<TopProductosDTO[]>('/reporte/top-productos', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener top 10 productos", error);
        throw error;
    }
};

export const getReporteTiempos = async (periodo: string, idSucursal: number | null, idEstado: number | null = null): Promise<TiemposProcesoDTO> => {
    try {
        const params: any = {
            dias: parseInt(periodo)
        };
        
        // Solo agregar idSucursal si es válido
        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }

        // Solo agregar idEstado si es válido
        if (idEstado !== null && idEstado > 0) {
            params.idEstado = idEstado;
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

export const getPedidosPorZona = async (
    fechaDesde?: string,
    fechaHasta?: string,
    idZona: number | null = null
): Promise<PedidosPorZonaDTO[]> => {
    try {
        const params: any = {};
        
        if (fechaDesde) {
            params.fechaDesde = fechaDesde;
        }
        
        if (fechaHasta) {
            params.fechaHasta = fechaHasta;
        }
        
        if (idZona !== null && idZona > 0) {
            params.idZona = idZona;
        }
        
        const response = await api.get<PedidosPorZonaDTO[]>('/reporte/pedidos-por-zona', {
            params: params
        });
        return response.data;
    } catch (error) {
        console.error("Error al obtener reporte de pedidos por zona", error);
        throw error;
    }
};

export const getReporteFormasPago = async (
    fechaDesde?: string,
    fechaHasta?: string,
    idSucursal: number | null = null
): Promise<ReporteFormasPagoDTO> => {
    try {
        const params: Record<string, string | number> = {};

        if (fechaDesde) {
            params.fechaDesde = fechaDesde;
        }

        if (fechaHasta) {
            params.fechaHasta = fechaHasta;
        }

        if (idSucursal !== null && idSucursal > 0) {
            params.idSucursal = idSucursal;
        }

        const response = await api.get<ReporteFormasPagoDTO>('/reporte/formas-pago', { params });

        return response.data;
    } catch (error) {
        console.error("Error al obtener reporte de formas de pago:", error);
        throw error;
    }
};

export const getReporteEncuestaSatisfaccion = async (): Promise<ReporteEncuestaSatisfaccionDTO> => {
    try {
        const response = await api.get<ReporteEncuestaSatisfaccionDTO>('/reporte/encuesta-satisfaccion');
        return response.data;
    } catch (error) {
        console.error("Error al obtener reporte de encuesta de satisfacción:", error);
        const params: any = {};
        if (fechaDesde) params.fechaDesde = fechaDesde;
        if (fechaHasta) params.fechaHasta = fechaHasta;
        if (idSucursal !== null && idSucursal > 0) params.idSucursal = idSucursal;
        const response = await api.get<ReporteFormasPagoDTO>('/reporte/formas-pago', { params });
        return response.data;
    } catch (error) {
        console.error('Error al obtener reporte de formas de pago', error);
        throw error;
    }
};

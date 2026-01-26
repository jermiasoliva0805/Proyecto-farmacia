import { api } from './api';
import {
    OrderSummaryDTO,
    AssignOperatorDTO,
    AssignDeliveryDTO,
    ChangeOrderStatusDTO,
} from '@/types/pedido.types';

export const pedidosService = {
    /**
     * Obtiene los pedidos filtrados desde el controlador de C#
     */
    async getFilteredOrders(filters: any): Promise<OrderSummaryDTO[]> {
        const params: any = {};
        
        // 1. Mapeo de nombres de estados a IDs (Deben coincidir con tu DB)
        const estadoMap: { [key: string]: number } = {
            'Sin preparar': 1,
            'Preparar pedido': 2,
            'Demorado': 3,
            'Listo para despachar': 4,
            'Despachando': 5,
            'En camino': 6,
            'Entregado': 7,
            'Entrega fallida': 8,
            'Cancelado': 9
        };

        // 2. Construcción de parámetros para el OrderFilterDTO de C#
        if (filters.search) {
            params.Search = filters.search;
        }
        
        if (filters.estado && filters.estado !== 'Todos') {
            params.IDEstadoDePedido = estadoMap[filters.estado];
        }
        
        // Si hay un ID de usuario (operario o cadete)
        const idUsuario = filters.idOperario || filters.idCadete || filters.idUsuario;
        if (idUsuario) {
            params.IDUsuario = idUsuario;
        }

        if (filters.fechaDesde) params.FechaDesde = filters.fechaDesde;
        if (filters.fechaHasta) params.FechaHasta = filters.fechaHasta;

        // 3. Llamada a la API
        // Nota: Asegúrate de que la ruta coincida con el [Route] de tu Controller
        const response = await api.get<OrderSummaryDTO[]>('/FiltrarPedidos/reporte', {
            params: params,
        });

        return response.data;
    },

    async asignarOperario(data: AssignOperatorDTO): Promise<void> {
        await api.patch('/orders/asignar-operario', data);
    },

    async asignarCadete(data: AssignDeliveryDTO): Promise<void> {
        await api.patch('/orders/asignar-cadete', data);
    },

    async cambiarEstado(data: ChangeOrderStatusDTO): Promise<void> {
        const payload = {
            IDPedido: data.idPedido,
            IDNuevoEstado: data.idNuevoEstado,
            IDUsuario: data.idUsuario,
            Observaciones: data.observaciones || "",
            MotivoCancelacion: data.idNuevoEstado === 8 ? data.observaciones : null
        };
        await api.put(`/orders/${data.idPedido}/estado`, payload);
    },

    async getPendientesOperario(): Promise<OrderSummaryDTO[]> {
        const response = await api.get<OrderSummaryDTO[]>('/orders/pendientes-operario');
        return response.data;
    },

    async getPendientesCadete(): Promise<OrderSummaryDTO[]> {
        const response = await api.get<OrderSummaryDTO[]>('/orders/pendientes-cadete');
        return response.data;
    }
};
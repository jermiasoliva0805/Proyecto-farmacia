import { api } from './api';
import {
    OrderSummaryDTO,
    AssignOperatorDTO,
    AssignDeliveryDTO,
    ChangeOrderStatusDTO,
} from '@/types/pedido.types';

export const pedidosService = {
    // --- NUEVOS MÉTODOS FILTRADOS ---

    async getPendientesOperario(): Promise<OrderSummaryDTO[]> {
        const response = await api.get<OrderSummaryDTO[]>('/orders/pendientes-operario');
        return response.data;
    },

    async getPendientesCadete(): Promise<OrderSummaryDTO[]> {
        const response = await api.get<OrderSummaryDTO[]>('/orders/pendientes-cadete');
        return response.data;
    },

    // --- MÉTODOS EXISTENTES CORREGIDOS ---

    async getFilteredOrders(filters: any): Promise<OrderSummaryDTO[]> {
        const cleanFilters: any = {};
        
        // Mapeo exacto de los nombres de los botones a IDs de la base de datos
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

        // 1. Filtro de búsqueda (ID o Nombre)
        if (filters.search) {
            cleanFilters.search = filters.search;
        }
        
        // 2. Filtro de Estado (Si es 'Todos' no se envía el parámetro para que traiga todos)
        if (filters.estado && filters.estado !== 'Todos') {
            const idEstado = estadoMap[filters.estado];
            if (idEstado) {
                cleanFilters.idEstado = idEstado;
            }
        }
        
        // 3. Filtros de Usuario (Operario o Cadete)
        if (filters.idOperario) cleanFilters.idOperario = filters.idOperario;
        if (filters.idCadete) cleanFilters.idCadete = filters.idCadete;
        
        // 4. Filtros de Fecha
        if (filters.fechaDesde) cleanFilters.fechaDesde = filters.fechaDesde;
        if (filters.fechaHasta) cleanFilters.fechaHasta = filters.fechaHasta;

        // 5. Filtro de Usuario logueado (para vistas de Operario/Cadete)
        if (filters.idUsuario) cleanFilters.idUsuario = filters.idUsuario;

        const response = await api.get<OrderSummaryDTO[]>('/filtrarpedidos/reporte', {
            params: cleanFilters,
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

    async getPedidosByRol(rol: string, userId: number, otrosFiltros: any = {}): Promise<OrderSummaryDTO[]> {
        let filters = { ...otrosFiltros };
        // Si el usuario no es admin, forzamos que solo vea lo suyo
        if (rol === 'Operario' || rol === 'Cadete') {
            filters.idUsuario = userId;
        }
        return this.getFilteredOrders(filters);
    },
};
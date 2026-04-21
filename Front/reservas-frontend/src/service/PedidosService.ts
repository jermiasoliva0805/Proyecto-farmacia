import { api } from './api';
import {
    OrderSummaryDTO,
    AssignOperatorDTO,
    AssignDeliveryDTO,
    ChangeOrderStatusDTO,
} from '@/types/pedido.types';

export const pedidosService = {
    async getFilteredOrders(filters: any): Promise<OrderSummaryDTO[]> {
        const params: any = {};
        
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

        if (filters.search) params.Search = filters.search;
        
        if (filters.idEstadoDePedido && filters.idEstadoDePedido > 0) {
            params.IDEstadoDePedido = filters.idEstadoDePedido;
        } 
        else if (filters.estadoNombre && filters.estadoNombre !== 'Todos') {
            params.IDEstadoDePedido = estadoMap[filters.estadoNombre];
        } else if (filters.estado && filters.estado !== 'Todos') {
            params.IDEstadoDePedido = estadoMap[filters.estado];
        }
        
        if (filters.idOperario) params.IDUsuario = filters.idOperario;
        else if (filters.idCadete) params.IDUsuario = filters.idCadete;
        else if (filters.idUsuario) params.IDUsuario = filters.idUsuario;

        if (filters.fechaDesde) params.fechaDesde = filters.fechaDesde;
        if (filters.fechaHasta) params.fechaHasta = filters.fechaHasta;

        console.log("== DEBUG INICIO PETICIÓN ==");
        console.log("Enviando parámetros al Backend:", params);

        try {
            const response = await api.get<OrderSummaryDTO[]>('/Orders/reporte', {
                params: params,
            });

            console.log("Respuesta Exitosa del Servidor:", response.data);
            
            if (response.data.length === 0) {
                console.warn("Aviso: El backend respondió con una lista VACÍA.");
            }

            return response.data;
        } catch (error: any) {
            console.error("== ERROR EN GET_FILTERED_ORDERS ==");
            
            if (error.response) {
                // El servidor respondió con un status fuera del rango 2xx
                console.error("Status Code:", error.response.status);
                console.error("Data de error del Backend:", error.response.data);
                
                if (error.response.status === 404) {
                    console.error("Error 404: No se encontró la ruta /Orders/reporte. Verifica el OrdersController.");
                }
                if (error.response.status === 401 || error.response.status === 403) {
                    console.error("Error de Permisos: No estás autorizado para ver este reporte.");
                }
            } else if (error.request) {
                // La petición se hizo pero no hubo respuesta
                console.error("Error de Red: No se recibió respuesta del servidor. ¿Está el backend corriendo?");
            } else {
                console.error("Error Mensaje:", error.message);
            }
            
            throw error;
        }
    },

    async getPedidosByRol(rol: string, userId: number, otrosFiltros: any = {}): Promise<OrderSummaryDTO[]> {
        let filters = { ...otrosFiltros };
        if (rol === 'Operario') {
            filters.idOperario = userId;
        } else if (rol === 'Cadete') {
            filters.idCadete = userId;
        }
        return this.getFilteredOrders(filters);
    },

    async getPendientesOperario(): Promise<OrderSummaryDTO[]> {
        const response = await api.get<OrderSummaryDTO[]>('/Orders/pendientes-operario');
        return response.data;
    },

    async getPendientesCadete(): Promise<OrderSummaryDTO[]> {
        const response = await api.get<OrderSummaryDTO[]>('/Orders/pendientes-cadete');
        return response.data;
    },

    async asignarOperario(data: AssignOperatorDTO): Promise<void> {
        await api.patch('/Orders/asignar-operario', data);
    },

    async asignarCadete(data: AssignDeliveryDTO): Promise<void> {
        const payload = {
            PedidoId: data.pedidoId,
            CadeteId: data.cadeteId
        };
        await api.patch('/Orders/asignar-cadete', payload);
    },

    async cambiarEstado(data: ChangeOrderStatusDTO): Promise<void> {
        const payload = {
            IDPedido: data.idPedido,
            IDNuevoEstado: data.idNuevoEstado,
            IDUsuario: data.idUsuario,
            Observaciones: data.observaciones // ✅ Incluida para auditoría
        };
        await api.put(`/Orders/${data.idPedido}/estado`, payload);
        
    },
    // Agregamos una función específica para el CU25 (Proceso de Armado)
    async iniciarPreparacion(idPedido: number, idUsuario: number): Promise<void> {
    return this.cambiarEstado({
        idPedido: idPedido,
        idNuevoEstado: 2, // Según tu estadoMap: 'Preparar pedido'
        idUsuario: idUsuario,
        observaciones: "El operario ha comenzado a armar el paquete."
    });
    },
    
    async createOrder(orderData: any): Promise<any> {
        try {
            // Normalizar los nombres de propiedades (convertir camelCase a PascalCase)
            const detalles = (orderData.Detalles || orderData.detalles || []).map((d: any) => ({
                IDProducto: d.IDProducto || d.idProducto,
                Cantidad: d.Cantidad || d.cantidad,
                PrecioUnitario: d.PrecioUnitario || d.precioUnitario
            }));

            const payload: any = {
                IDCliente: orderData.IDCliente || orderData.idCliente,
                IDSucursal: orderData.IDSucursal || orderData.idSucursal,
                IDUsuario: orderData.IDUsuario || orderData.idUsuario,
                FormaDePago: orderData.FormaDePago || orderData.formaDePago,
                NombreCliente: orderData.NombreCliente,
                Telefono: orderData.Telefono,
                Email: orderData.Email,
                Direccion: orderData.Direccion,
                PuntoRetiro: orderData.PuntoRetiro,
                Detalles: detalles
            };

            // ✅ Incluir ZonaId si viene del frontend (con nullish coalescing)
            const zonaId = orderData.ZonaId ?? orderData.zonaId;
            if (zonaId !== null && zonaId !== undefined) {
                payload.ZonaId = zonaId;
            }

            console.log('📤 Enviando al backend:', payload);
            const response = await api.post('/Orders', payload);
            return response.data;
        } catch (error: any) {
            console.error("Error al crear pedido:", error.response?.data || error.message);
            throw error;
        }
    },

    async cancelarPedido(data: any): Promise<void> {
        const payload = {
            PedidoId: data.idPedido,
            MotivoCancelacionId: data.motivoCancelacionId || 1, // Por defecto motivo genérico
            Justificacion: data.justificacion || data.observaciones || '',
            UsuarioId: String(data.idUsuario)
        };
        await api.post('/Orders/cancelar', payload);
    }

};
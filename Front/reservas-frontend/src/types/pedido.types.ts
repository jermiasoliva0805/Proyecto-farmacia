export interface OrderSummaryDTO {
    operarioNombre: string;
    idPedido: number;
    fecha: string;
    total: number;
    estadoNombre: string;
    idEstadoDePedido: number;
    clienteNombre: string;
    responsableNombre: string;
    fechaEntregaEstimada: string;
    estaDemorado: boolean;
    fechaEntregaReal?: string;
    // ✅ Agregado para vincular los productos del catálogo
    detalles?: OrderDetailDTO[];
    intentosEntregaFallida: number;
}

export interface OrderDetailDTO {
    idProducto: number;
    nombreProducto?: string; // ✅ Para mostrar "Oneblade", "Dadatina", etc.
    cantidad: number;
    precioUnitario: number;
}

export interface CreateOrderDTO {
    idCliente: number;
    idSucursal: number;
    idUsuario: number;
    formaDePago: string;
    detalles: OrderDetailDTO[];
}

export interface AssignOperatorDTO {
    pedidoId: number;
    operarioId: number;
}

export interface AssignDeliveryDTO {
    pedidoId: number;
    cadeteId: number;
}

export interface ChangeOrderStatusDTO {
    idPedido: number;
    idNuevoEstado: number;
    idUsuario: number;
    observaciones?: string;
    motivoCancelacion?: string;
}

export interface OrderFilterDTO {
    estado?: string;
    search?: string;
    idEstadoDePedido?: number;
    idUsuario?: number;
    idCliente?: number;
    fechaDesde?: string;
    fechaHasta?: string;
}

export interface RankingClienteDTO {
    nombreCliente: string;
    cantidadPedidos: number;
    gastoTotal: number;
    ticketPromedio: number;
    ultimaCompra: string;
}

export interface ClienteFacturacionDTO {
    nombreCliente: string;
    totalFacturado: number;
    cantidadPedidos: number;
    ticketPromedio: number;
}
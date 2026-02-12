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
}

export interface OrderDetailDTO {
    idProducto: number;
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
    idNuevoEstado: number;   // ✅ ahora es number, consistente con el back
    idUsuario: number;
    observaciones?: string;
    motivoCancelacion?: string;
}

export interface OrderFilterDTO {
    estado?: string;          // ✅ corregido: debe ser string, no boolean
    search?: string;          // ✅ corregido: debe ser string, no any
    idEstadoDePedido?: number;
    idUsuario?: number;
    idCliente?: number;
    fechaDesde?: string;
    fechaHasta?: string;
}

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
    detalles?: OrderDetailDTO[];
    intentosEntregaFallida: number;
    // ✅ CU25: Para detectar si ya inició el armado
    fechaInicioArmado?: string;
    fechaFinArmado?: string;
    zonaNombre?: string;
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

export interface DetalleCancelacionDTO {
    motivoCancelacion: string;
    cantidad: number;
    porcentajeDelTotal: number;
}

export interface ReportePedidosCanceladosDTO {
    totalPedidosCancelados: number;
    porcentajeDelTotal: number;
    montoTotalCancelado: number;
    detallePorMotivo: DetalleCancelacionDTO[];
}

export interface CancelacionPorMotivoDTO {
    motivo: string;
    cantidad: number;
    porcentaje: number;
    montoPerdido: number;
}

export interface ReporteCancelacionesPorMotivoDTO {
    totalPedidos: number;
    totalCancelados: number;
    porcentajeCancelacion: number;
    ingresosPerdidos: number;
    entregasFallidas: number;
    porcentajeEntregasFallidas: number;
    principalMotivo: string;
    detalleMotivos: CancelacionPorMotivoDTO[];
}

export interface TopProductosDTO {
    idProducto: number;
    nombreProducto: string;
    unidadesVendidas: number;
    porcentaje: number;
    precioPromedio: number;
}

export interface FaseProcesoDTO {
    nombre: string;
    tiempoPromedio: number;
    color: string;
}

export interface TiemposProcesoDTO {
    fases: FaseProcesoDTO[];
    puntoCritico: string;
    tiempoPuntoCritico: number;
    eficienciaDespacho: number;
    totalPedidos: number;
    detalles: DetalleTiempoProcesoDTO[];
}

export interface DetalleTiempoProcesoDTO {
    idPedido: number;
    espera: number;
    preparacion: number;
    despacho: number;
    viaje: number;
    estadoFinal: string;
    esAlertaDespacho: boolean;
}

export interface PedidosPorZonaDTO {
    zonaId: number;
    nombreZona: string;
    cantidadPedidos: number;
    porcentaje: number;
    totalRecaudado: number;
}

export interface DetalleFormaPagoDTO {
    formaDePago: string;
    cantidadOperaciones: number;
    porcentaje: number;
    montoTotal: number;
}

export interface ReporteFormasPagoDTO {
    totalOperaciones: number;
    totalMonto: number;
    distribucionFormasPago: DetalleFormaPagoDTO[];
}

export interface OpcionRespuestaEncuestaDTO {
    respuesta: string;
    cantidad: number;
    porcentaje: number;
}

export interface PreguntaEncuestaDTO {
    pregunta: string;
    totalRespuestas: number;
    opciones: OpcionRespuestaEncuestaDTO[];
}

export interface ReporteEncuestaSatisfaccionDTO {
    totalRespuestas: number;
    preguntas: PreguntaEncuestaDTO[];
}

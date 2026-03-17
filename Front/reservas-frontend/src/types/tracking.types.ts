export interface TrackingHistoryItemDTO {
    nombreEstado: string;
    fechaHora: string;
    responsable: string;
    motivoCancelacion?: string;
    observaciones?: string;
    IntentosEntregaFallida?: number; // Número de intento fallido (1, 2, 3)
    IntentosMax?: number; // Máximo de intentos permitidos
}

export interface OrderTrackingDTO {
    idPedido: number;
    estadoActual: string;
    ultimaActualizacion: string;
    historial: TrackingHistoryItemDTO[];
}
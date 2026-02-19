export interface RankingClienteDTO {
    nombreCliente: string;
    cantidadPedidos: number;
    gastoTotal: number;
    ticketPromedio: number;
    ultimaCompra: string; // La fecha llega como texto (ISO) y React la transforma
}
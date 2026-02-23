namespace Proyecto_farmacia.DTOs
{
    public class ClienteFacturacionDTO
    {
        public required string NombreCliente { get; set; }
        public decimal TotalFacturado { get; set; }
        public int CantidadPedidos { get; set; }
        
        // Ticket Promedio calculado automáticamente
        public decimal TicketPromedio => CantidadPedidos > 0 ? TotalFacturado / CantidadPedidos : 0;
    }
}
namespace Back.DTOs
{
    public class PedidosPorZonaDTO
    {
        public int ZonaId { get; set; }
        public string NombreZona { get; set; } = string.Empty;
        public int CantidadPedidos { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal TotalRecaudado { get; set; }
        
        // Nuevas propiedades para detalle
        public int EntregasExitosas { get; set; }
        public int EntregasFallidas { get; set; }
        public decimal PorcentajeEfectividad { get; set; }
    }
}

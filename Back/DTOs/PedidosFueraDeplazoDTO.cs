namespace Back.DTOs
{
    /// <summary>
    /// DTO para el reporte de entregas fuera de plazo
    /// </summary>
    public class PedidosFueraDeplazoDTO
    {
        // ==================== MÉTRICAS ====================
        public int TotalEntregas { get; set; }
        public int EntregasTardías { get; set; }
        public double RetrasoPromedioDías { get; set; }

        // ==================== DETALLES ====================
        public List<DetallePedidoFueraDeplazo> Detalles { get; set; } = new();
    }

    /// <summary>
    /// Detalle de un pedido que fue entregado fuera de plazo
    /// </summary>
    public class DetallePedidoFueraDeplazo
    {
        public int IDPedido { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string NombreCadete { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaEstimada { get; set; }
        public DateTime FechaEntrega { get; set; }
        public double RetrasoDías { get; set; } // Diferencia en días hábiles
        public int IntentosEntregaFallida { get; set; }
    }
}

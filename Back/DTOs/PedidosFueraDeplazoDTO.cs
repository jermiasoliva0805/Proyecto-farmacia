namespace Back.DTOs
{
    /// <summary>
    /// DTO para el reporte de entregas fuera de plazo.
    /// Solo incluye pedidos con estado Entregado (7) que superaron la fecha estimada.
    /// </summary>
    public class PedidosFueraDeplazoDTO
    {
        // ==================== MÉTRICAS ====================
        public int TotalEntregas { get; set; }
        public int EntregasTardías { get; set; }
        public int RetrasoPromedioDías { get; set; }
 
        // ==================== DETALLES ====================
        public List<DetallePedidoFueraDeplazo> Detalles { get; set; } = new();
    }
 
    /// <summary>
    /// Detalle de un pedido entregado fuera de plazo.
    /// </summary>
    public class DetallePedidoFueraDeplazo
    {
        public int IDPedido { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string NombreCadete { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaEstimada { get; set; }
        public DateTime FechaEntrega { get; set; }
 
        /// <summary>
        /// Retraso calculado en días hábiles (no días corridos).
        /// </summary>
        public int RetrasoDías { get; set; }
 
        public int IntentosEntregaFallida { get; set; }
 
        // ── NUEVOS: contexto sobre el subestado demorado ─────────────────────
        /// <summary>
        /// Indica si el pedido fue marcado automáticamente como "demorado"
        /// en algún momento antes de su entrega.
        /// </summary>
        public bool FueMarcadoDemorado { get; set; }
 
        /// <summary>
        /// Timestamp de cuándo fue marcado demorado. Null si nunca lo fue.
        /// </summary>
        public DateTime? FechaMarcadoDemorado { get; set; }
        // ─────────────────────────────────────────────────────────────────────
    }
}
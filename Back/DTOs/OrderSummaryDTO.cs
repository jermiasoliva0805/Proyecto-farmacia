namespace Back.DTOs
{
    public class OrderSummaryDTO
    {
        public int IDPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public int IDEstadoDePedido { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;
 
        // ── NUEVO: rol del responsable (para la campana de notificaciones) ───
        public string ResponsableRol { get; set; } = string.Empty;
        // ─────────────────────────────────────────────────────────────────────
 
        public int? ResponsableId { get; set; }
        public DateTime? FechaEntregaReal { get; set; }
        public DateTime FechaEntregaEstimada { get; set; }
 
        // Subestado demorado (flag, no estado principal)
        public bool EstaDemorado { get; set; }
 
        // ── NUEVO: cuándo fue marcado demorado (para la campana) ─────────────
        public DateTime? FechaMarcadoDemorado { get; set; }
        // ─────────────────────────────────────────────────────────────────────
 
        public int IntentosEntregaFallida { get; set; }
        public string? FechaInicioArmado { get; set; }
        public string? FechaFinArmado { get; set; }
        public string? ZonaNombre { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? LocalidadNombre { get; set; }
        public string? CodigoPostalEntrega { get; set; }
        public string? EstadoActual { get; set; }
    }
}
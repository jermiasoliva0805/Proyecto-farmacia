namespace Back.DTOs
{
    public class OrderFilterDTO
    {
        public int? IDEstadoDePedido { get; set; }
 
        public int? IDUsuario { get; set; }
 
        public DateTime? FechaDesde { get; set; }
 
        public DateTime? FechaHasta { get; set; }
 
        public string? Search { get; set; }
 
        // ── NUEVO: filtra únicamente pedidos con subestado demorado ──────────
        // Puede combinarse con IDEstadoDePedido para filtrar, por ejemplo,
        // "pedidos En camino que además están demorados".
        public bool? SoloDemorados { get; set; }
        // ─────────────────────────────────────────────────────────────────────
    }
}
 
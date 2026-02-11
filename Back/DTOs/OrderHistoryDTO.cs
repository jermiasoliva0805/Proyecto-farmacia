namespace Back.DTOs
{
    public class OrderHistoryDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public DateTime FechaCambio { get; set; }
        public string? Motivo { get; set; }
        public string? NombreUsuario { get; set; } // Nombre del operario/cadete que hizo el cambio
    }
}
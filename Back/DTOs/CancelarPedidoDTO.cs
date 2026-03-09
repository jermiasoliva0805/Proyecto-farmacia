namespace Back.DTOs
{
    public class CancelarPedidoDTO
    {
        public int PedidoId { get; set; }
        public int MotivoCancelacionId { get; set; }
        public string? Justificacion { get; set; }
        public string? UsuarioId { get; set; }
    }
}
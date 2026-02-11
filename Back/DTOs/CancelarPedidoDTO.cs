namespace Back.DTOs
{
    public class CancelarPedidoDTO
    {
        public int PedidoId { get; set; }
        public int MotivoCancelacionId { get; set; }
        public string Justificacion { get; set; } // Opcional para el RF16
        public string UsuarioId { get; set; } // ID del usuario que realiza la cancelación
    }
}
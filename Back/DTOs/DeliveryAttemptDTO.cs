namespace Back.DTOs
{
    public class DeliveryAttemptDTO
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public DateTime FechaIntento { get; set; }
        public bool EsExitoso { get; set; }
        public string? Comentario { get; set; } // Ejemplo: "Domicilio sin moradores"
        public string? RutaFotoEvidencia { get; set; } // Por si el cadete sube foto
    }
}
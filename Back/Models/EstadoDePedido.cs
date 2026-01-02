using Back.Models;

namespace Back.Models
{
    public class EstadoDePedido
    {
        public int IDEstadoDePedido { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string motivo_cancelacion { get; set; } = string.Empty;

        // Relaciones: Un estado puede estar en muchos pedidos y en muchos historiales
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<HistorialDeEstados> Historiales { get; set; } = new List<HistorialDeEstados>();
    }
}
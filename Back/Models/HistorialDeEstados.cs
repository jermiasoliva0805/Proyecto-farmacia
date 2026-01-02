using Back.Models;

namespace Back.Models
{
    public class HistorialDeEstados
    {
        public int IDHistorialEstados { get; set; }
        public DateTime fecha_hora_inicio { get; set; }
        public DateTime? fecha_hora_fin { get; set; } // Puede ser null si es el estado actual

        // Foreign Keys
        public int IDUsuario { get; set; }
        public int IDPedido { get; set; }
        public int IDEstadoDePedido { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Pedido Pedido { get; set; } = null!;
        public EstadoDePedido EstadoDePedido { get; set; } = null!;
    }
}
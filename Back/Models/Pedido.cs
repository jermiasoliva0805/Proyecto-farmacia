using Back.Models;

namespace Back.Models
{
    public class Pedido
    {
        public int IDPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string FormaDePago { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;

        // Los campos que faltaban:
        public DateTime? FechaEntregaReal { get; set; }
        public DateTime FechaEntregaEstimada { get; set; }
        public TimeSpan? HoraEntregaReal { get; set; }
        public TimeSpan HoraEntregaEstimada { get; set; }

        // Foreign Keys
        public int IDCliente { get; set; }
        public int IDEstadoDePedido { get; set; }
        public int IDUsuario { get; set; }
        public int IDSucursal { get; set; }

        // Propiedades de Navegación (Relaciones)
        public Cliente Cliente { get; set; } = null!;
        public EstadoDePedido EstadoDePedido { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public Sucursal Sucursal { get; set; } = null!;
        public ICollection<DetalleDePedido> Detalles { get; set; } = new List<DetalleDePedido>();
    }
}
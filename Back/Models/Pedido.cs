using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    public class Pedido
    {
        public int IDPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string FormaDePago { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public string DireccionEntrega { get; set; } = string.Empty;
        public int IDLocalidad { get; set; }

        [Required]
        public string Estado { get; set; } = "Sin preparar";

        public DateTime? FechaEntregaReal { get; set; }
        public DateTime FechaEntregaEstimada { get; set; }
        public TimeSpan? HoraEntregaReal { get; set; }
        public TimeSpan HoraEntregaEstimada { get; set; }

        // Foreign Keys
        public int IDCliente { get; set; }
        public int IDEstadoDePedido { get; set; }
        public int IDUsuario { get; set; }
        public int IDSucursal { get; set; }

        // Propiedades de Navegación
        public Cliente Cliente { get; set; } = null!;
        public EstadoDePedido EstadoDePedido { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public Sucursal Sucursal { get; set; } = null!;

        public ICollection<DetalleDePedido> Detalles { get; set; } = new List<DetalleDePedido>();
        public ICollection<HistorialDeEstados> HistorialDeEstados { get; set; } = new List<HistorialDeEstados>();

        // Corregido: Aseguramos que el nombre sea el que usaremos en el Repo o viceversa
        public int? MotivoCancelacionId { get; set; }

        [ForeignKey("MotivoCancelacionId")]
        public virtual MotivoCancelacion? MotivoCancelacion { get; set; }

        public string? JustificacionCancelacion { get; set; }
    }
}
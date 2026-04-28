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
        public string? CodigoPostalEntrega { get; set; }
        public string? ReferenciaEntrega { get; set; }
        public int IDLocalidad { get; set; }
 
        [Required]
        public string Estado { get; set; } = "Sin preparar";
 
        public DateTime? FechaEntregaReal { get; set; }
        public DateTime FechaEntregaEstimada { get; set; }
        public TimeSpan? HoraEntregaReal { get; set; }
        public TimeSpan HoraEntregaEstimada { get; set; }
 
        // ── NUEVO: subestado "demorado" como flag independiente ──────────────
        // Reemplaza la lógica de sobreescribir Estado/IDEstadoDePedido con "Demorado".
        // Un pedido puede estar demorado sin importar su estado principal.
        public bool EsDemorado { get; set; } = false;
 
        // Timestamp de cuándo fue marcado demorado por última vez (útil para auditoría)
        public DateTime? FechaMarcadoDemorado { get; set; }
        // ─────────────────────────────────────────────────────────────────────
 
        // Foreign Keys
        public int IDCliente { get; set; }
        public int IDEstadoDePedido { get; set; }
        public int IDUsuario { get; set; }
        public int IDSucursal { get; set; }
        public int? ZonaId { get; set; }
 
        public int IntentosEntregaFallida { get; set; } = 0;
 
        // CU25: Registro de tiempo de armado
        public DateTime? FechaInicioArmado { get; set; }
        public DateTime? FechaFinArmado { get; set; }
 
        // Propiedades de Navegación
        public Cliente Cliente { get; set; } = null!;
        public EstadoDePedido EstadoDePedido { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public Sucursal Sucursal { get; set; } = null!;
 
        [ForeignKey("ZonaId")]
        public Zona? Zona { get; set; } = null;
 
        public ICollection<DetalleDePedido> Detalles { get; set; } = new List<DetalleDePedido>();
        public ICollection<HistorialDeEstados> HistorialDeEstados { get; set; } = new List<HistorialDeEstados>();
 
        public int? MotivoCancelacionId { get; set; }
        [ForeignKey("MotivoCancelacionId")]
        public virtual MotivoCancelacion? MotivoCancelacion { get; set; }
        public string? JustificacionCancelacion { get; set; }
    }
}
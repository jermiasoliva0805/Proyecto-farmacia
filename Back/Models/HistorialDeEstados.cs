using Back.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace Back.Models
{
    public class HistorialDeEstados
    {
    
        
            [Key]
            public int IDHistorialEstados { get; set; }
            public DateTime fecha_hora_inicio { get; set; }
            public DateTime? fecha_hora_fin { get; set; } // Puede ser null si es el estado actual
            public string? Observaciones { get; set; }

            public int IDUsuario { get; set; }
            [ForeignKey("IDUsuario")]
            public Usuario? Usuario { get; set; } = null!;

            public int IDPedido { get; set; }
            [ForeignKey("IDPedido")]
            public Pedido Pedido { get; set; } = null!;

            public int IDEstadoDePedido { get; set; }
            [ForeignKey("IDEstadoDePedido")]
            public EstadoDePedido EstadoDePedido { get; set; } = null!;
        
    }
}
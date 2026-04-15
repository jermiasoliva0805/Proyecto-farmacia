using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Models
{
    public class Barrio
   
    {
            [Key]
            public int IDBarrio { get; set; }
            public string Nombre { get; set; } = string.Empty;

            // FK a Localidad (Córdoba)
            public int IDLocalidad { get; set; }
            public Localidad Localidad { get; set; } = null!;

            // FK a Zona (para mapear barrio a zona de reparto)
            public int? ZonaId { get; set; }

            [ForeignKey("ZonaId")]
            public Zona? Zona { get; set; } = null;

            // Relación: En un barrio viven muchos clientes
            public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
    }
}

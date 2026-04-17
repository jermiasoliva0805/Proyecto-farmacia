using System.ComponentModel.DataAnnotations;

namespace Back.Models
{
    /// <summary>
    /// Zona de reparto en Córdoba.
    /// Define áreas geográficas para la asignación de cadetes.
    /// </summary>
    public class Zona
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty; // Ej: "Zona Norte", "Centro"

        // Relaciones de Navegación
        public ICollection<Usuario> Cadetes { get; set; } = new List<Usuario>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<Barrio> Barrios { get; set; } = new List<Barrio>();
    }
}

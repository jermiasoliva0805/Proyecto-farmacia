using Back.Models;

namespace Back.Models
{
    public class Cliente
    {
        public int IDCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string DNI { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;

        // FK y Propiedad de Navegación a Localidad
        public int IDLocalidad { get; set; }
        public Localidad Localidad { get; set; } = null!;

        // Un cliente tiene muchos pedidos
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
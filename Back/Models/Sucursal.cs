using Back.Models;

namespace Back.Models
{
    public class Sucursal
    {
        public int IDSucursal { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public string Dirección { get; set; } = string.Empty;
        public string Teléfono { get; set; } = string.Empty;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }
}
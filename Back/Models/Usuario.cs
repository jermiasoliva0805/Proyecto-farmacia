namespace Back.Models
{
    public class Usuario
    {
        public int IDUsuario { get; set; } 
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty; // "Usuario" en el diagrama
        public string Contraseña { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;

        // Relación con Sucursal (CF en  diagrama)
        public int IDSucursal { get; set; }
        public Sucursal Sucursal { get; set; } = null!;

        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
        public ICollection<IntentoDeEntrega> IntentosDeEntrega { get; set; } = new List<IntentoDeEntrega>();
    }
}

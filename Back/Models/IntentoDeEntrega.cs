using Back.Models;

namespace Back.Models
{
    public class IntentoDeEntrega
    {
        public int IDIntentoDeEntrega { get; set; }
        public DateTime FechaDeIntento { get; set; }
        public string Resultado { get; set; } = string.Empty;
        public string RazonDeFallo { get; set; } = string.Empty;
        public string URL_Foto_Verificacion { get; set; } = string.Empty;
        public TimeSpan Hora { get; set; }

        // Foreign Keys
        public int IDUsuario { get; set; }
        public int IDPedido { get; set; }

        // Relaciones
        public Usuario Usuario { get; set; } = null!;
        public Pedido Pedido { get; set; } = null!;
    }
}
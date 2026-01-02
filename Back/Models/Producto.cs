namespace Back.Models
{
    public class Producto
    {
        public int IDProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int CantidadProducto { get; set; }
        public decimal PrecioProducto { get; set; }
        public ICollection<DetalleDePedido> Detalles { get; set; } = new List<DetalleDePedido>();
    }
}
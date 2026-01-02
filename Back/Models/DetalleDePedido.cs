using Back.Models;

namespace Back.Models
{
    public class DetalleDePedido
    {
        public int IDDetalleDePedido { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public int IDPedido { get; set; }
        public Pedido Pedido { get; set; } = null!;

        public int IDProducto { get; set; }
        public Producto Producto { get; set; } = null!;
    }
}
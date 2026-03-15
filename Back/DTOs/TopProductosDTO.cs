namespace Back.DTOs
{
    public class TopProductosDTO
    {
        public int IDProducto { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int UnidadesVendidas { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal PrecioPromedio { get; set; }
    }
}

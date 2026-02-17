namespace Back.DTOs
{
    public class ReporteOperarioDTO
    {
        public string NombreOperario { get; set; } = string.Empty;
        public double TiempoPromedioMinutos { get; set; }
        public int TotalPedidosArmados { get; set; }
    }
}
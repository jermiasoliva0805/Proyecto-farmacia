namespace Back.DTOs
{
    public class ReporteOperarioDTO
    {
        public string NombreOperario { get; set; } = string.Empty;
        public int PedidosTotales { get; set; }
        public int DentroUmbral { get; set; } // Pedidos de < 30 min
        public int FueraUmbral { get; set; }  // Pedidos de > 30 min
        public double TiempoPromedioMinutos { get; set; }
        public double PorcentajeEficiencia { get; set; }
    }
}
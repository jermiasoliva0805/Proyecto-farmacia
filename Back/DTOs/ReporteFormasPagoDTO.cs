namespace Back.DTOs
{
    public class ReporteFormasPagoDTO
    {
        public int TotalOperaciones { get; set; }
        public decimal TotalMonto { get; set; }
        public List<DetalleFormaPagoDTO> DistribucionFormasPago { get; set; } = new List<DetalleFormaPagoDTO>();
    }

    public class DetalleFormaPagoDTO
    {
        public string FormaDePago { get; set; } = string.Empty;
        public int CantidadOperaciones { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal MontoTotal { get; set; }
    }
}

namespace Back.DTOs
{
    public class CancelacionPorMotivoDTO
    {
        public string Motivo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal MontoPerdido { get; set; }
    }

    public class ReporteCancelacionesPorMotivoDTO
    {
        public int TotalPedidos { get; set; }
        public int TotalCancelados { get; set; }
        public decimal PorcentajeCancelacion { get; set; }
        public decimal IngresosPerdidos { get; set; }
        public string PrincipalMotivo { get; set; } = string.Empty;
        public List<CancelacionPorMotivoDTO> DetalleMotivos { get; set; } = new List<CancelacionPorMotivoDTO>();
    }
}

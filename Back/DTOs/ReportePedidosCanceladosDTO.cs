namespace Back.DTOs
{
    public class ReportePedidosCanceladosDTO
    {
        public int TotalPedidosCancelados { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
        public decimal MontoTotalCancelado { get; set; }
        public List<DetalleCancelacionDTO> DetallePorMotivo { get; set; } = new List<DetalleCancelacionDTO>();
    }

    public class DetalleCancelacionDTO
    {
        public string MotivoCancelacion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PorcentajeDelTotal { get; set; }
    }
}

namespace Back.DTOs
{
    public class DetalleTiempoProcesoDTO
    {
        public int IdPedido { get; set; }
        public double Espera { get; set; }
        public double Preparacion { get; set; }
        public double Despacho { get; set; }
        public double Viaje { get; set; }
        public string EstadoFinal { get; set; }
        public bool EsAlertaDespacho { get; set; }
    }
}

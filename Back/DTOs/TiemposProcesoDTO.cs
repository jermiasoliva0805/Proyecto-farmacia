namespace Back.DTOs
{
    public class TiemposProcesoDTO
    {
        public List<FaseProcesoDTO> Fases { get; set; }
        public string PuntoCritico { get; set; }
        public double TiempoPuntoCritico { get; set; }
        public int EficienciaDespacho { get; set; }
        public int TotalPedidos { get; set; }
        public List<DetalleTiempoProcesoDTO> Detalles { get; set; }
    }
}

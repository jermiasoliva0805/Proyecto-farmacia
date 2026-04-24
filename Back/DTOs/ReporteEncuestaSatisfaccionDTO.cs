namespace Back.DTOs
{
    public class ReporteEncuestaSatisfaccionDTO
    {
        public int CantidadClientesRespondieron { get; set; }
        public int CantidadTotalRespuestas { get; set; }
        public List<PreguntaEncuestaDTO> Preguntas { get; set; } = new();
    }

    public class PreguntaEncuestaDTO
    {
        public string Pregunta { get; set; } = string.Empty;
        public int TotalRespuestas { get; set; }
        public List<OpcionRespuestaEncuestaDTO> Opciones { get; set; } = new();
    }

    public class OpcionRespuestaEncuestaDTO
    {
        public string Respuesta { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }
}

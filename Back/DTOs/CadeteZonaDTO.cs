using System.Text.Json.Serialization;

namespace Back.DTOs
{
    public class CadeteZonaDTO
    {
        [JsonPropertyName("idCadete")]
        public int IDCadete { get; set; }
        
        [JsonPropertyName("nombreCadete")]
        public string NombreCadete { get; set; } = string.Empty;
        
        [JsonPropertyName("totalPedidosAsignados")]
        public int TotalPedidosAsignados { get; set; }
        
        [JsonPropertyName("entregasExitosas")]
        public int EntregasExitosas { get; set; }
        
        [JsonPropertyName("entregasFallidas")]
        public int EntregasFallidas { get; set; }
        
        [JsonPropertyName("totalRecaudado")]
        public decimal TotalRecaudado { get; set; }
        
        [JsonPropertyName("porcentajeEfectividad")]
        public decimal PorcentajeEfectividad { get; set; }
    }
}

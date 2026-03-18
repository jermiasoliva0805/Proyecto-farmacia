using System;
using System.Collections.Generic;

namespace Back.DTOs
{
    public class OrderTrackingDTO
    {
        public int IDPedido { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public DateTime UltimaActualizacion { get; set; }

        // Lista de los movimientos históricos
        public List<TrackingHistoryItemDTO> Historial { get; set; } = new List<TrackingHistoryItemDTO>();
    }

    public class TrackingHistoryItemDTO
    {
        public string NombreEstado { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Responsable { get; set; } = string.Empty; // Nombre + Apellido del Usuario
        public string? MotivoCancelacion { get; set; }
        public string? Observaciones { get; set; }
        public int IntentosEntregaFallida { get; set; } = 0; // Número de intento fallido (1, 2, 3)
        public int IntentosMax { get; set; } = 3; // Máximo de intentos permitidos
    }
}

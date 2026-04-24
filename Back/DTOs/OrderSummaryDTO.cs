using System;

namespace Back.DTOs
{
    public class OrderSummaryDTO
    {
        public int IDPedido { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        public int IDEstadoDePedido { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ResponsableNombre { get; set; } = string.Empty;

        public DateTime FechaEntregaEstimada { get; set; }
        public DateTime? FechaEntregaReal { get; set; }

        // Nuevo campo para mostrar intentos fallidos
        public int IntentosEntregaFallida { get; set; }

        // Campo adicional que ya tenías para marcar demora
        public bool EstaDemorado { get; set; }

        // ✅ CU25: Fechas de armado para detectar si ya inició
        public string? FechaInicioArmado { get; set; }
        public string? FechaFinArmado { get; set; }

        // Zona de reparto
        public string? ZonaNombre { get; set; }

        // Datos de entrega para navegación (Maps)
        public string? DireccionEntrega { get; set; }
        public string? LocalidadNombre { get; set; }
        public string? CodigoPostalEntrega { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Back.DTOs
{
    /// <summary>
    /// DTO para la etiqueta de envío del paquete
    /// Contiene solo datos de envío, sin los detalles de productos
    /// </summary>
    public class OrderLabelDTO
    {
        public int IDPedido { get; set; }
        public DateTime Fecha { get; set; }
        
        // Datos del Cliente
        public string? ClienteNombre { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteEmail { get; set; }
        
        // Datos de Envío
        public string? ClienteDireccion { get; set; }
        public string? CodigoPostal { get; set; }
        public string? ReferenciaEntrega { get; set; } // Ej: "Entre Calle X y Calle Y"
        public string? ClienteLocalidadBarrio { get; set; }
        
        // Método y detalles de envío
        public string? MetodoEnvio { get; set; }
        public string? PuntoRetiro { get; set; }
    }
}

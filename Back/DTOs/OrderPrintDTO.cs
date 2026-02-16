using System;
using System.Collections.Generic;

namespace Back.DTOs
{
    public class OrderPrintDTO
    {
        public int IDPedido { get; set; }
        public DateTime Fecha { get; set; }
        public string? FormaDePago { get; set; }
        public decimal Total { get; set; }
        public string? MetodoEnvio { get; set; }
        public string? PuntoRetiro { get; set; }

        public string? ClienteNombre { get; set; }
        public string? ClienteDireccion { get; set; }
        public string? ClienteLocalidadBarrio { get; set; }
        public string? ClienteDNI { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteEmail { get; set; }

        public List<OrderDetailItemDTO> Productos { get; set; } = new List<OrderDetailItemDTO>();
    }

    public class OrderDetailItemDTO
    {
        public int Cantidad { get; set; }
        public string? ProductoNombre { get; set; }
        public string? SKU { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
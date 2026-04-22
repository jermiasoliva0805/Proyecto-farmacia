using System.ComponentModel.DataAnnotations;

namespace Back.DTOs
{
    public class CreateOrderDTO
    {
        // IDCliente es opcional porque el cliente puede ser nuevo
        public int? IDCliente { get; set; }

        [Required]
        public int IDSucursal { get; set; }

        [Required]
        public int IDUsuario { get; set; }

        [Required]
        public string FormaDePago { get; set; }

        // Nuevos campos para datos de envío
        public string? NombreCliente { get; set; }
        
        public string? Direccion { get; set; }
        
        public string? Telefono { get; set; }
        
        public string? Email { get; set; }
        
        public string? ReferenciaEntrega { get; set; }

        // Zona de reparto (se captura desde el barrio seleccionado)
        public int? ZonaId { get; set; }

        // Aquí van los detalles del pedido (RF17)
        public List<OrderDetailDTO> Detalles { get; set; } = new List<OrderDetailDTO>();
    }
}
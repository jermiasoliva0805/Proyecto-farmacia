using System.ComponentModel.DataAnnotations;

namespace Back.DTOs
{
    public class UserDTO
    {
        public int IDUsuario { get; set; }
        public int Id { get; set; }
        public string Usuario { get; set; }

        // Concatenación para reportes (Ref: RF5 - Generación de valor en reportes).
        public string NombreCompleto { get; set; }
        public string Email { get; set; }

        // Ref: RF8 - El rol determina qué estados de pedido puede cambiar el usuario (RF1, RF1.1).
        public string Rol { get; set; }

        // Ref: Entidad Sucursal - "Permite segmentar la operación por ubicación".
        public string NombreSucursal { get; set; }
    }
}

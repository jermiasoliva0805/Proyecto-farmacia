using System.ComponentModel.DataAnnotations;

namespace Back.DTOS
{
    public class UpdateUserDTO
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? UsuarioNombre { get; set; }
        public string? Mail { get; set; }
        public string? Contraseña { get; set; }
        public string? Rol { get; set; }
        public int? IDSucursal { get; set; }
        public int? ZonaId { get; set; } // Zona de reparto para cadetes
    }
}
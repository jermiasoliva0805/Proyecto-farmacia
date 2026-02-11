using System.ComponentModel.DataAnnotations;
namespace Back.DTOS
{
    public class RegisterDTO
    {
        public string Nombre { get; set; }
        public string UsuarioNombre { get; set; }
        public string Mail { get; set; }
        public string Contraseña { get; set; }
        public string Rol { get; set; }
        public int IDSucursal { get; set; }
    }
}
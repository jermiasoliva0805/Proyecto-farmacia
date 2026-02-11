using System.ComponentModel.DataAnnotations;

namespace Back.DTOS
{
    public class UpdateUserDTO
    {
        public int IDUsuario { get; set; }
        public string Nombre { get; set; }
        public string UsuarioNombre { get; set; }
        public string Mail { get; set; }
        public string Rol { get; set; }
        public int IDSucursal { get; set; }
    }
}


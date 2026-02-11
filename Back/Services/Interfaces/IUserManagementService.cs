using Back.DTOs;
using Back.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Services.Interfaces // O namespace Back.Services
{
    public interface IUserManagementService
    {
        // Para llenar los Selects del Front (RF18, RF19)
        // Ej: Traer todos los usuarios que sean "Cadete"
        Task<IEnumerable<UserDTO>> GetUsersByRoleAsync(string role);

        // Funcionalidad de Admin: Resetear la contraseña de otro usuario
        Task<bool> AdminResetPasswordAsync(int userId, string newPassword);

        // Si necesitas lógica para cambiar el rol de un usuario específicamente
        Task<bool> ChangeUserRoleAsync(int userId, string newRole);
    }
}
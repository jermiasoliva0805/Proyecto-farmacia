using Back.DTOs;
using Back.DTOS; // Asegúrate de que sea DTOS o DTOs según tu carpeta
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Services
{
    public interface IUserService
    {
        // --- TUS MÉTODOS EXISTENTES (Lectura) ---
        // Ref: RF13 - Listado de usuarios/responsables
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(int id);

        // --- MÉTODOS NUEVOS (Escritura) ---

        // RF8: Registrar un nuevo usuario (Devolvemos UserDTO para no exponer la pass)
        Task<UserDTO> RegisterUserAsync(RegisterDTO registerDto);

        // Modificar datos (Devolvemos bool para saber si tuvo éxito)
        Task<bool> UpdateUserAsync(int id, UpdateUserDTO updateDto);

        // Eliminar usuario
        Task<bool> DeleteUserAsync(int id);
    }
}
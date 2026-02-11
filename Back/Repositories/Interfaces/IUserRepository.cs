using Back.Models; // Asegúrate si tu entidad está en .Models o .Entities
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Repositories
{
    public interface IUserRepository
    {
        // --- Lo que ya tenías ---
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<bool> CreateAsync(Usuario usuario);
        Task<bool> UpdateAsync(Usuario usuario);
        Task<bool> DeleteAsync(int id);

        // --- LO NUEVO (Lógica de Auth y Validación) ---

        // Para el Login (necesitamos buscar por string, no por ID)
        Task<Usuario?> GetByUsernameAsync(string username);

        // Para evitar duplicados en el Register (validaciones)
        Task<bool> UserExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }
}
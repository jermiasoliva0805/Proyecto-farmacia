using Back.Data;
using Back.Models;
using Microsoft.EntityFrameworkCore;

namespace Back.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> Login(string username, string password)
        {
            // Ref: RF7 - El sistema debe permitir la autenticación de usuarios.
            // Buscamos por UsuarioNombre
            var user = await _context.Usuarios
                .Include(u => u.Sucursal) // Importante para el UserDTO después
                .FirstOrDefaultAsync(x => x.UsuarioNombre == username);

            // Si no existe el usuario, retornamos null
            if (user == null) return null;

            // Intentamos verificar con BCrypt (para usuarios nuevos con contraseña hasheada)
            bool passwordValid = false;
            try
            {
                passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Contraseña);
            }
            catch
            {
                // Si BCrypt falla (ej: la contraseña está en texto plano), intentamos comparación directa
                passwordValid = user.Contraseña == password;

                // Si la contraseña era texto plano y es válida, la hasheamos ahora para futuras autenticaciones
                if (passwordValid)
                {
                    user.Contraseña = BCrypt.Net.BCrypt.HashPassword(password);
                    _context.Usuarios.Update(user);
                    await _context.SaveChangesAsync();
                }
            }

            if (!passwordValid)
                return null;

            return user;
        }

        public async Task<bool> UserExists(string username)
        {
            return await _context.Usuarios.AnyAsync(x => x.UsuarioNombre == username);
        }
    }
}
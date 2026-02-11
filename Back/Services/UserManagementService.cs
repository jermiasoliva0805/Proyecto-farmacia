using AutoMapper;
using Back.DTOs;
using Back.DTOS;
using Back.Repositories;
using Back.Services.Interfaces; // Asegúrate de tener este using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserManagementService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetUsersByRoleAsync(string role)
        {
            // 1. Traemos todos los usuarios
            // Nota: Si tuvieras miles de usuarios, sería mejor filtrar en el Repositorio (SQL),
            // pero para una farmacia (menos de 100 empleados) filtrar en memoria está bien.
            var allUsers = await _userRepository.GetAllAsync();

            // 2. Filtramos por el Rol solicitado (ignorando mayúsculas/minúsculas)
            var filteredUsers = allUsers
                .Where(u => u.Rol.Trim().Equals(role, StringComparison.OrdinalIgnoreCase));

            // 3. Mapeamos a DTO
            return _mapper.Map<IEnumerable<UserDTO>>(filteredUsers);
        }

        public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
        {
            // 1. Buscamos al usuario
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // 2. Encriptamos la nueva contraseña manualmente
            user.Contraseña = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // 3. Guardamos usando el método Update del repositorio
            return await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, string newRole)
        {
            // 1. Validar que el rol sea válido (opcional, según tu lógica)
            var rolesValidos = new List<string> { "Administrador", "Operario", "Cadete" };
            if (!rolesValidos.Contains(newRole)) throw new Exception("Rol no válido");

            // 2. Buscar usuario
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // 3. Actualizar rol
            user.Rol = newRole;

            // 4. Guardar
            return await _userRepository.UpdateAsync(user);
        }
    }
}
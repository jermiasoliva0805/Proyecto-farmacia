using AutoMapper;
using Back.DTOs;
using Back.DTOS;
using Back.Models;
using Back.Repositories;
using BCrypt.Net; // Necesario para el HashPassword
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // ==========================================
        // TUS MÉTODOS DE LECTURA (Ya existían)
        // ==========================================

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var usuarios = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(usuarios);
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var usuario = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserDTO>(usuario);
        }

        // ==========================================
        // MÉTODOS NUEVOS (Lógica de Negocio y Seguridad)
        // ==========================================

        public async Task<UserDTO> RegisterUserAsync(RegisterDTO registerDto)
        {
            // 1. Validaciones de Negocio (RF8)
            // Verificamos que no se repita el usuario ni el mail
            if (await _userRepository.UserExistsAsync(registerDto.UsuarioNombre))
            {
                throw new Exception("El nombre de usuario ya está en uso.");
            }

            if (await _userRepository.EmailExistsAsync(registerDto.Mail))
            {
                throw new Exception("El correo electrónico ya está registrado.");
            }

            // 2. Mapeo de DTO a Entidad
            var usuarioEntity = _mapper.Map<Usuario>(registerDto);

            // 3. SEGURIDAD: Hasheo de contraseña
            // Nunca guardamos texto plano. BCrypt maneja el 'Salt' automáticamente.
            usuarioEntity.Contraseña = BCrypt.Net.BCrypt.HashPassword(registerDto.Contraseña);

            // 4. Guardar en Base de Datos
            var resultado = await _userRepository.CreateAsync(usuarioEntity);

            if (!resultado)
            {
                throw new Exception("Error al guardar el usuario en la base de datos.");
            }

            // 5. Devolver el DTO (sin la contraseña)
            return _mapper.Map<UserDTO>(usuarioEntity);
        }

        public async Task<bool> UpdateUserAsync(int id, UpdateUserDTO updateDto)
        {
            // 1. Verificar que el usuario exista
            var usuarioExistente = await _userRepository.GetByIdAsync(id);
            if (usuarioExistente == null) return false;

            // 2. Actualizar campos
            // Usamos AutoMapper para pasar los datos del DTO a la entidad existente.
            // Como UpdateUserDTO NO tiene el campo 'Contraseña', la contraseña actual NO se toca.
            _mapper.Map(updateDto, usuarioExistente);

            // 3. Guardar cambios
            return await _userRepository.UpdateAsync(usuarioExistente);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            // La lógica de verificación ya suele estar en el repositorio o se maneja con el retorno bool
            return await _userRepository.DeleteAsync(id);
        }
    }
}
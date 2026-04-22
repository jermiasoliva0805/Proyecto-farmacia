using AutoMapper;
using Back.DTOs;
using Back.DTOS;
using Back.Models;
using Back.Repositories;
using Back.Repositories.Interfaces;
using BCrypt.Net; // Necesario para el HashPassword
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Back.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IPedidoRepository pedidoRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
        }

        // ==========================================
        // TUS MÉTODOS DE LECTURA (Ya existían)
        // ==========================================

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(int? currentUserId = null)
        {
            var usuarios = await _userRepository.GetAllAsync();
            
            // CAMBIO 1: Si se proporciona un ID del usuario actual (encargado), excluirlo de la lista
            if (currentUserId.HasValue)
            {
                usuarios = usuarios.Where(u => u.IDUsuario != currentUserId.Value);
            }
            
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
            _mapper.Map(updateDto, usuarioExistente);

            // 3. Si se proporciona una nueva contraseña, hashearla
            if (!string.IsNullOrEmpty(updateDto.Contraseña))
            {
                usuarioExistente.Contraseña = BCrypt.Net.BCrypt.HashPassword(updateDto.Contraseña);
            }

            // 4. Guardar cambios
            return await _userRepository.UpdateAsync(usuarioExistente);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            // Obtener usuario para verificar su rol y pedidos
            var usuario = await _userRepository.GetByIdWithPedidosAsync(id);
            if (usuario == null) return false;

            // VALIDACIÓN 1: Si es cadete, verificar si tiene pedidos "En Camino" (estado 6)
            if (usuario.Rol == "Cadete")
            {
                // Buscar si tiene pedidos con estado "En Camino"
                var pedidosEnCamino = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO
                {
                    IDUsuario = id,
                    IDEstadoDePedido = 6 // En Camino
                });

                if (pedidosEnCamino != null && pedidosEnCamino.Any())
                {
                    // Lanzar excepción bloqueante
                    throw new InvalidOperationException(
                        $"No se puede eliminar el cadete {usuario.Nombre} {usuario.Apellido} porque tiene {pedidosEnCamino.Count()} pedido(s) en estado 'En Camino'. " +
                        "El proceso logístico es irreversible en esta etapa. Espere a que se completen las entregas antes de eliminarlo."
                    );
                }
            }

            // VALIDACIÓN 2: No permitir eliminar operario si tiene pedidos asignados
            // CAMBIO 3: Un operario no se puede eliminar si tiene CUALQUIER pedido asignado
            // Estados permitidos: 7 (Entregado), 8 (Cancelado), o sin pedidos
            // Estados bloqueantes: 1 (Sin preparar), 2 (Preparar), 3 (Demorado), 4 (Listo), 5 (En ruta), 6 (En Camino)
            if (usuario.Rol == "Operario")
            {
                var tienePedidosAsignados = usuario.Pedidos.Any(p => 
                    p.IDEstadoDePedido >= 1 && p.IDEstadoDePedido <= 6 // Cualquier estado antes de entregar o cancelar
                );

                if (tienePedidosAsignados)
                {
                    throw new Exception($"No se puede eliminar al operario {usuario.Nombre} {usuario.Apellido} porque tiene pedidos asignados. "
                        + "El operario debe completar o cancelar todos los pedidos antes de ser eliminado.");
                }
            }

            // Si pasa ambas validaciones, proceder con soft delete (preserva historial)
            return await _userRepository.DeleteAsync(id);
        }
    }
}
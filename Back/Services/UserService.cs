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

            // CAMBIO 4: Validación especial para cadetes que intenten cambiar zona
            if (usuarioExistente.Rol == "Cadete" && updateDto.ZonaId.HasValue && 
                updateDto.ZonaId.Value != usuarioExistente.ZonaId)
            {
                // El cadete intenta cambiar de zona. Validar que no tenga pedidos sin entregar
                var pedidosSinEntregar = await _pedidoRepository.GetFilteredOrdersAsync(new OrderFilterDTO
                {
                    IDUsuario = id,
                    IDEstadoDePedido = 6 // En Camino (pedidos que aún no entregó)
                });

                if (pedidosSinEntregar != null && pedidosSinEntregar.Any())
                {
                    throw new InvalidOperationException(
                        $"No se puede cambiar la zona del cadete {usuarioExistente.Nombre} {usuarioExistente.Apellido} " +
                        "porque tiene pedidos en entrega. Debe completar todas las entregas antes de cambiar de zona."
                    );
                }
            }

            // 2. Actualizar solo los campos que se proporcionan (no-null)
            if (!string.IsNullOrEmpty(updateDto.Nombre))
                usuarioExistente.Nombre = updateDto.Nombre;
            
            if (!string.IsNullOrEmpty(updateDto.Apellido))
                usuarioExistente.Apellido = updateDto.Apellido;
            
            if (!string.IsNullOrEmpty(updateDto.UsuarioNombre))
                usuarioExistente.UsuarioNombre = updateDto.UsuarioNombre;
            
            if (!string.IsNullOrEmpty(updateDto.Mail))
                usuarioExistente.Mail = updateDto.Mail;
            
            if (!string.IsNullOrEmpty(updateDto.Rol))
                usuarioExistente.Rol = updateDto.Rol;
            
            if (updateDto.IDSucursal.HasValue)
                usuarioExistente.IDSucursal = updateDto.IDSucursal.Value;
            
            if (updateDto.ZonaId.HasValue)
                usuarioExistente.ZonaId = updateDto.ZonaId.Value;

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

            // VALIDACIÓN 2: No permitir eliminar operario si tiene pedidos en armado
            // Estados bloqueantes para Operario: 2 (Preparar), 3 (Demorado)
            // Estado 4 (Listo para despachar) ya no le pertenece, puede eliminarse
            if (usuario.Rol == "Operario")
            {
                var tienePedidosEnArmado = usuario.Pedidos.Any(p => 
                    p.IDEstadoDePedido == 2 || p.IDEstadoDePedido == 3 // Solo Preparar y Demorado
                );

                if (tienePedidosEnArmado)
                {
                    throw new Exception($"No se puede eliminar al operario {usuario.Nombre} {usuario.Apellido} porque tiene pedidos en armado. "
                        + "El operario debe completar o cancelar todos los pedidos en preparación antes de ser eliminado.");
                }
            }

            // Si pasa ambas validaciones, proceder con soft delete (preserva historial)
            return await _userRepository.DeleteAsync(id);
        }
    }
}
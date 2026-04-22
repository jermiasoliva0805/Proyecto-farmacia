using Microsoft.AspNetCore.Mvc;
using Back.Services;
using Back.DTOs;
using Back.DTOS;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsuariosController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. LISTAR TODOS (Para la tabla del ABM)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUsuarios()
        {
            // CAMBIO 1: Obtener el ID del usuario autenticado desde el token JWT
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? currentUserId = null;
            
            if (int.TryParse(currentUserIdClaim, out int userId))
            {
                currentUserId = userId;
            }

            // Pasar el ID del usuario actual para que se excluya de la lista
            var users = await _userService.GetAllUsersAsync(currentUserId);
            return Ok(users);
        }

        // 2. OBTENER UNO SOLO
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound(new { message = "Usuario no encontrado." });
            return Ok(user);
        }

        // 3. CREAR USUARIO (Alta de Personal)
        [HttpPost]
        public async Task<IActionResult> CreateUsuario([FromBody] RegisterDTO registerDto)
        {
            try 
            {
                var newUser = await _userService.RegisterUserAsync(registerDto);
                return CreatedAtAction(nameof(GetUsuario), new { id = newUser.IDUsuario }, newUser);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 4. EDITAR USUARIO (Modificación) - Con validación de seguridad
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UpdateUserDTO updateDto)
        {
            // CAMBIO 2: Lógica mejorada de autorización
            // Obtener el ID y Rol del usuario autenticado desde el token JWT
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            // Permitir editar solo si:
            // 1. Es el mismo usuario (puede editar su propio perfil), O
            // 2. Es Encargado (puede editar a cualquiera)
            bool esUsuarioActual = currentUserId == id;
            bool esEncargado = currentUserRole?.Equals("Encargado", StringComparison.OrdinalIgnoreCase) ?? false;

            if (!esUsuarioActual && !esEncargado)
            {
                return Unauthorized(new 
                { 
                    message = "No tienes permiso para editar este usuario. Solo puedes modificar tu propio perfil.",
                    errorCode = "UNAUTHORIZED_EDIT"
                });
            }

            try
            {
                var result = await _userService.UpdateUserAsync(id, updateDto);
                if (!result) return NotFound(new { message = "Usuario no encontrado." });
                
                return Ok(new { message = "Usuario actualizado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = "CADETE_HAS_ACTIVE_DELIVERIES" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar usuario: " + ex.Message });
            }
        }

        // 5. ELIMINAR USUARIO (Baja)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);
                if (!result) return NotFound(new { message = "Usuario no encontrado." });

                return Ok(new { message = "Usuario eliminado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                // Error de validación de negocio: cadete con pedidos en camino
                return BadRequest(new 
                { 
                    message = ex.Message,
                    errorCode = "CADETE_HAS_ACTIVE_DELIVERIES"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar usuario: " + ex.Message });
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Back.Services;
using Back.DTOs;
using Back.DTOS;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> GetUsuarios()
        {
            var users = await _userService.GetAllUsersAsync();
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

        // 4. EDITAR USUARIO (Modificación)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UpdateUserDTO updateDto)
        {
            var result = await _userService.UpdateUserAsync(id, updateDto);
            if (!result) return NotFound(new { message = "No se pudo actualizar. Usuario no encontrado." });
            
            return Ok(new { message = "Usuario actualizado correctamente." });
        }

        // 5. ELIMINAR USUARIO (Baja)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound(new { message = "Usuario no encontrado." });

            return Ok(new { message = "Usuario eliminado correctamente." });
        }
    }
}
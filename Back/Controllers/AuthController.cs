using Microsoft.AspNetCore.Mvc;
using Back.Services;
using Back.DTOs;

namespace Back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            // El servicio busca al usuario y lo mapea a UserDTO
            var user = await _authService.Login(loginDto);

            if (user == null)
                return Unauthorized(new { message = "Credenciales inválidas." });

            // Generamos el token JWT
            var token = _authService.GenerateToken(user);

            // Retornamos el token y los datos del usuario (normalizados en minúscula)
            return Ok(new
            {
                token = token,
                user = new
                {
                    id = user.Id,
                    usuario = user.Usuario,
                    nombreCompleto = user.NombreCompleto,
                    email = user.Email,
                    rol = user.Rol,   // <-- ahora se devuelve como "rol"
                    nombreSucursal = user.NombreSucursal
                }
            });
        }
    }
}

using AutoMapper;
using Back.DTOs;
using Back.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Back.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository authRepository, IMapper mapper, IConfiguration config)
        {
            _authRepository = authRepository;
            _mapper = mapper;
            _config = config;
        }

        public async Task<UserDTO?> Login(LoginDTO loginDto)
        {
            var usuario = await _authRepository.Login(loginDto.Usuario, loginDto.Password);
            if (usuario == null) return null;
            return _mapper.Map<UserDTO>(usuario);
        }

        public string GenerateToken(UserDTO user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (user.Id <= 0)
                throw new InvalidOperationException("No se puede generar JWT: el usuario no tiene Id válido.");

            if (string.IsNullOrWhiteSpace(user.Usuario))
                throw new InvalidOperationException("No se puede generar JWT: el usuario no tiene 'Usuario'.");

            if (string.IsNullOrWhiteSpace(user.Rol))
                throw new InvalidOperationException("No se puede generar JWT: el usuario no tiene 'Rol'.");

            // Azure App Service (Linux):
            // AppSettings__Token => AppSettings:Token
            var tokenSecret = _config["AppSettings:Token"] ?? _config["JWT_TOKEN"];

            if (string.IsNullOrWhiteSpace(tokenSecret))
                throw new InvalidOperationException(
                    "Falta configurar el secreto JWT. Configure 'AppSettings__Token' (recomendado) o 'JWT_TOKEN' en el App Service.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Usuario),
                new Claim(ClaimTypes.Role, user.Rol),
            };

            // Este claim te estaba rompiendo si NombreSucursal era null
            if (!string.IsNullOrWhiteSpace(user.NombreSucursal))
                claims.Add(new Claim("Sucursal", user.NombreSucursal));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}

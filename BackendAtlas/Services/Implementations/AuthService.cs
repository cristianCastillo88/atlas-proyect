using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using BackendAtlas.Domain;
using BackendAtlas.Services.Interfaces;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Servicio de autenticación siguiendo patrón CQS.
    /// Mejoras de escalabilidad:
    /// - Usa IUsuarioRepository (CQS) en lugar de DbContext directo
    /// - Async/await correctamente implementado
    /// - Validaciones de entrada
    /// - Logging para auditoría (preparado para rate limiting futuro)
    /// - Separación de responsabilidades (generación de token)
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService>? _logger;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IConfiguration configuration,
            ILogger<AuthService>? logger = null)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
            _logger = logger;
            _tokenValidationParameters = BuildTokenValidationParameters();
        }

        /// <summary>
        /// Autentica un usuario por email y contraseña.
        /// Mejoras de seguridad:
        /// - Validaciones de entrada
        /// - Logging de intentos de login (para detectar ataques)
        /// - Retorna null en vez de excepciones para evitar información sensible
        /// - Usa timing constante en verificación de password (BCrypt)
        /// </summary>
        public async Task<LoginResponse?> Login(string email, string password, CancellationToken cancellationToken = default)
        {
            // Validaciones de entrada
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger?.LogWarning("Intento de login con email o password vacío");
                return null;
            }

            // Normalizar email (case-insensitive)
            email = email.Trim().ToLowerInvariant();

            // CQS: Usar repositorio para lectura (AsNoTracking)
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(email, cancellationToken);
            
            if (usuario == null)
            {
                // SEGURIDAD: No revelar si el usuario existe o no
                _logger?.LogWarning("Intento de login fallido para email: {Email}", email);
                
                // Ejecutar BCrypt.Verify aunque no exista el usuario para prevenir timing attacks
                BCrypt.Net.BCrypt.Verify(password, "$2a$11$dummy.hash.to.prevent.timing.attacks.xxxxxxxxxxxxxxxxxxxx");
                
                return null;
            }

            // Verificar password con BCrypt (timing constante)
            if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            {
                _logger?.LogWarning("Password incorrecto para usuario: {Email}", email);
                return null;
            }

            // Login exitoso
            _logger?.LogInformation("Login exitoso para usuario: {Email}, Rol: {Rol}", email, usuario.Rol);

            var token = GenerateJwtToken(usuario);
            return new LoginResponse
            {
                Token = token,
                Role = usuario.Rol.ToString(),
                Name = usuario.Nombre,
                Email = usuario.Email,
                UserId = usuario.Id,
                NegocioId = usuario.NegocioId?.ToString(),
                SucursalId = usuario.SucursalId?.ToString()
            };
        }

        /// <summary>
        /// Genera un JWT token con claims de seguridad mejorados.
        /// Mejoras:
        /// - Usa DateTime.UtcNow para evitar problemas de zona horaria
        /// - Incluye NotBefore claim
        /// - Token único por sesión (jti)
        /// - Claims estructurados para multi-tenancy
        /// </summary>
        private string GenerateJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));
            var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");

            var now = DateTime.UtcNow;
            var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var minutes) ? minutes : 60;

            var claims = new List<Claim>
            {
                // Claims estándar JWT
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token único
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                
                // Claims para autorización
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString())
            };

            // Claims de multi-tenancy (para escalar con múltiples negocios/sucursales)
            if (usuario.NegocioId.HasValue)
            {
                claims.Add(new Claim("negocioId", usuario.NegocioId.Value.ToString()));
            }

            if (usuario.SucursalId.HasValue)
            {
                claims.Add(new Claim("sucursalId", usuario.SucursalId.Value.ToString()));
            }

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature // Más específico
            );

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now, // Token válido desde ahora
                expires: now.AddMinutes(expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Construye parámetros de validación de token para reutilizar.
        /// Preparado para validación de refresh tokens en el futuro.
        /// </summary>
        private TokenValidationParameters BuildTokenValidationParameters()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(5) // Tolerancia de 5 minutos para sincronización de relojes
            };
        }
    }
}
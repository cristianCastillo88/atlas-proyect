using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using BackendAtlas.Domain;
using BackendAtlas.Services.Interfaces;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using BackendAtlas.Configuration;

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
        private readonly IPasswordPolicyService _passwordPolicyService;
        private readonly IEmailService _emailService;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IUnitOfWork _unitOfWork; // Asumiendo que existe, si no, lo inyectaremos en controlador o aquí

        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthService>? _logger;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            IPasswordPolicyService passwordPolicyService,
            IEmailService emailService,
            IPasswordResetTokenRepository passwordResetTokenRepository,
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService>? logger = null)
        {
            _usuarioRepository = usuarioRepository;
            _passwordPolicyService = passwordPolicyService;
            _emailService = emailService;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _unitOfWork = unitOfWork;
            _jwtSettings = jwtSettings.Value;
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
                _logger?.LogWarning(SecurityLogEvents.LoginFailed, "Login fallido (usuario no encontrado): {Email}", email);
                
                // Ejecutar BCrypt.Verify aunque no exista el usuario para prevenir timing attacks
                BCrypt.Net.BCrypt.Verify(password, "$2a$11$dummy.hash.to.prevent.timing.attacks.xxxxxxxxxxxxxxxxxxxx");
                
                return null;
            }

            // Verificar password con BCrypt (timing constante)
            if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            {
                _logger?.LogWarning(SecurityLogEvents.LoginFailed, "Login fallido (password incorrecto): {Email}", email);
                return null;
            }

            // Login exitoso
            _logger?.LogInformation(SecurityLogEvents.LoginSuccess, "Login exitoso: {Email}, Rol: {Rol}, ID: {Id}", email, usuario.Rol, usuario.Id);

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

        public async Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, ct);
            if (usuario == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(passwordActual, usuario.PasswordHash))
            {
                _logger?.LogWarning(SecurityLogEvents.PasswordChangeFailed, "Fallo al cambiar password: Clave actual incorrecta. Usuario {Id}", usuarioId);
                return false;
            }

            var validacion = _passwordPolicyService.ValidatePassword(passwordNueva);
            if (!validacion.IsValid)
            {
                throw new ArgumentException($"Password no cumple políticas: {string.Join(", ", validacion.Errors)}");
            }

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);
            usuario.UltimaActualizacionPassword = DateTime.UtcNow;
            usuario.RequiereCambioPassword = false;

            _usuarioRepository.Actualizar(usuario);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger?.LogInformation(SecurityLogEvents.PasswordChangeSuccess, "Password cambiada exitosamente para usuario {Id}", usuarioId);
            return true;
        }

        public async Task SolicitarRecuperacionPasswordAsync(string email, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorEmailAsync(email, ct);
            
            if (usuario == null)
            {
                // Seguridad: Timing attack mitigation.
                await Task.Delay(new Random().Next(100, 300), ct); 
                return;
            }

            // 1. Invalidar tokens anteriores
            await _passwordResetTokenRepository.InvalidarTokensAnterioresDelUsuarioAsync(usuario.Id, ct);

            // 2. Generar token criptográficamente seguro y su hash
            var rawToken = GenerateSecureToken();
            var tokenHash = ComputeSha256Hash(rawToken);

            var tokenEntity = new PasswordResetToken
            {
                UsuarioId = usuario.Id,
                Token = tokenHash, // Guardamos HASH en BD
                FechaExpiracion = DateTime.UtcNow.AddHours(1),
                Usado = false,
                Usuario = usuario
            };

            await _passwordResetTokenRepository.CrearAsync(tokenEntity, ct);
            
            // 3. Enviar email con el token RAW (Usuario recibe la llave)
            await _emailService.EnviarEmailRecuperacionPasswordAsync(usuario.Email, usuario.Nombre, rawToken, ct);
            
            _logger?.LogInformation(SecurityLogEvents.PasswordResetRequested, "Solicitud recuperacion para {UserId} ({Email}) procesada.", usuario.Id, email);
        }

        public async Task<bool> RestablecerPasswordConTokenAsync(string rawToken, string nuevaPassword, CancellationToken ct = default)
        {
            // 1. Hashear el token recibido para buscarlo
            var tokenHash = ComputeSha256Hash(rawToken);

            var tokenEntity = await _passwordResetTokenRepository.ObtenerPorTokenAsync(tokenHash, ct);
            
            if (tokenEntity == null || !tokenEntity.EsValido())
            {
                _logger?.LogWarning(SecurityLogEvents.PasswordResetFailed, "Intento de restablecimiento con token inválido/expirado.");
                return false;
            }

            // 2. Transacción de Negocio
            using var transaction = await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var validacion = _passwordPolicyService.ValidatePassword(nuevaPassword);
                if (!validacion.IsValid)
                {
                    throw new ArgumentException($"Password no cumple políticas: {string.Join(", ", validacion.Errors)}");
                }

                // Actualizar password usuario
                var usuario = tokenEntity.Usuario;
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
                usuario.UltimaActualizacionPassword = DateTime.UtcNow;
                usuario.RequiereCambioPassword = false;

                _usuarioRepository.Actualizar(usuario);
                
                // Marcar token como usado
                await _passwordResetTokenRepository.MarcarComoUsadoAsync(tokenEntity.Id, ct);
                
                await _unitOfWork.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                
                _logger?.LogInformation(SecurityLogEvents.PasswordResetSuccess, "Password restablecida con token para usuario {UserId}", usuario.Id);
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<string> RestablecerPasswordPorAdminAsync(int usuarioId, CancellationToken ct = default)
        {
            var usuario = await _usuarioRepository.ObtenerPorIdAsync(usuarioId, ct);
            if (usuario == null)
            {
                throw new KeyNotFoundException($"Usuario {usuarioId} no encontrado");
            }

            var passwordTemporal = _passwordPolicyService.GenerateTemporaryPassword();
            // IMPORTANTE: Un admin setea password temporal -> Usuario DEBE cambiarla
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordTemporal);
            usuario.UltimaActualizacionPassword = DateTime.UtcNow;
            usuario.RequiereCambioPassword = true; 

            _usuarioRepository.Actualizar(usuario);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger?.LogWarning("Password restablecida para usuario {Id} por administrador", usuarioId);
            return passwordTemporal;
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
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var issuer = _jwtSettings.Issuer;
            var audience = _jwtSettings.Audience;

            var now = DateTime.UtcNow;
            var expirationMinutes = _jwtSettings.ExpirationMinutes > 0 ? _jwtSettings.ExpirationMinutes : 60;

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
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromMinutes(5) // Tolerancia de 5 minutos para sincronización de relojes
            };
        }

        // --- Helpers de Seguridad ---

        private static string GenerateSecureToken()
        {
            // Más seguro que Guid.NewGuid()
            var bytes = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-").Replace("/", "_").Replace("=", ""); // URL Safe Base64
        }

        private static string ComputeSha256Hash(string input)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
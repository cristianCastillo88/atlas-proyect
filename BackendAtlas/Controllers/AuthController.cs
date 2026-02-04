using System.Security.Claims;
using BackendAtlas.Services.Interfaces;
using BackendAtlas.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BackendAtlas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("AuthLimit")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.Login(request.Email, request.Password, cancellationToken);
            if (result == null)
            {
                // Seguridad: Mensaje genérico
                return Unauthorized(new { message = "Credenciales inválidas" });
            }
            return Ok(result);
        }

        [HttpPost("cambiar-password")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto, CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            try
            {
                var resultado = await _authService.CambiarPasswordAsync(
                    userId, 
                    dto.PasswordActual, 
                    dto.PasswordNueva, 
                    ct);
                
                if (!resultado)
                {
                    return BadRequest(new { message = "Contraseña actual incorrecta" });
                }
                
                return Ok(new { message = "Contraseña actualizada exitosamente" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("solicitar-recuperacion")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] SolicitarRecuperacionDto dto, CancellationToken ct = default)
        {
            try
            {
                await _authService.SolicitarRecuperacionPasswordAsync(dto.Email, ct);
                
                return Ok(new { 
                    message = "Si el email existe en nuestro sistema, recibirás instrucciones para recuperar tu contraseña." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en solicitud de recuperación para {Email}", dto.Email);
                return Ok(new { 
                    message = "Si el email existe en nuestro sistema, recibirás instrucciones para recuperar tu contraseña." 
                });
            }
        }

        [HttpPost("restablecer-con-token")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RestablecerConToken([FromBody] RestablecerConTokenDto dto, CancellationToken ct = default)
        {
            try
            {
                var resultado = await _authService.RestablecerPasswordConTokenAsync(
                    dto.Token, 
                    dto.NuevaPassword, 
                    ct);
                
                if (!resultado)
                {
                    return BadRequest(new { message = "Token inválido o expirado. Por favor solicita uno nuevo." });
                }
                
                return Ok(new { message = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("restablecer-por-admin")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RestablecerPorAdmin([FromBody] RestablecerPorAdminDto dto, CancellationToken ct = default)
        {
            try
            {
                var passwordTemporal = await _authService.RestablecerPasswordPorAdminAsync(dto.UsuarioId, ct);
                
                return Ok(new 
                {
                    message = "Contraseña restablecida exitosamente",
                    passwordTemporal = passwordTemporal,
                    requiereCambio = true
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

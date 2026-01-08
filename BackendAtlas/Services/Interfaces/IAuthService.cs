using System.Threading.Tasks;

namespace BackendAtlas.Services.Interfaces
{
    /// <summary>
    /// Servicio de autenticación y autorización.
    /// Futuras extensiones: RefreshToken, RevokeToken, ValidateToken
    /// </summary>
    public interface IAuthService
    {
        Task<LoginResponse?> Login(string email, string password, CancellationToken cancellationToken = default);
        
        // TODO: Implementar para escalabilidad
        // Task<LoginResponse?> RefreshToken(string refreshToken, CancellationToken cancellationToken = default);
        // Task RevokeToken(string token, CancellationToken cancellationToken = default);
        // Task<bool> ValidateToken(string token, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Respuesta de autenticación con información completa del usuario.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? NegocioId { get; set; }
        public string? SucursalId { get; set; }
        
        // TODO: Agregar para refresh token pattern
        // public string? RefreshToken { get; set; }
        // public DateTime ExpiresAt { get; set; }
    }
}
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
        // Gestión de Password
        Task<bool> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, CancellationToken ct = default);
        
        // Recuperación por Email
        Task SolicitarRecuperacionPasswordAsync(string email, CancellationToken ct = default);
        Task<bool> RestablecerPasswordConTokenAsync(string token, string nuevaPassword, CancellationToken ct = default);
        
        // Reset Admin
        Task<string> RestablecerPasswordPorAdminAsync(int usuarioId, CancellationToken ct = default);
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
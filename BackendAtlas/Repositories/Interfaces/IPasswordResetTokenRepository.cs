using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> ObtenerPorTokenAsync(string token, CancellationToken ct = default);
        Task CrearAsync(PasswordResetToken resetToken, CancellationToken ct = default);
        Task MarcarComoUsadoAsync(int tokenId, CancellationToken ct = default);
        Task InvalidarTokensAnterioresDelUsuarioAsync(int usuarioId, CancellationToken ct = default);
    }
}

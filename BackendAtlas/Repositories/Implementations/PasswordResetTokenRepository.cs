using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken?> ObtenerPorTokenAsync(string token, CancellationToken ct)
        {
            return await _context.PasswordResetTokens
                .AsNoTracking() // Read-only para validación
                .Include(t => t.Usuario) // Necesitamos el usuario asociado
                .FirstOrDefaultAsync(t => t.Token == token, ct);
        }

        public async Task CrearAsync(PasswordResetToken resetToken, CancellationToken ct)
        {
            await _context.PasswordResetTokens.AddAsync(resetToken, ct);
            // Nota: No llamamos a SaveChanges aquí, se maneja por UnitOfWork si existiera, o en el servicio.
            // Asumimos que el servicio llamará a _context.SaveChangesAsync() o similar.
            // Para simplicidad en este proyecto, guardamos aquí o inyectamos UnitOfWork
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarcarComoUsadoAsync(int tokenId, CancellationToken ct)
        {
            var token = await _context.PasswordResetTokens.FindAsync(new object[] { tokenId }, ct);
            if (token != null)
            {
                token.Usado = true;
                token.FechaUso = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task InvalidarTokensAnterioresDelUsuarioAsync(int usuarioId, CancellationToken ct)
        {
            var tokensActivos = await _context.PasswordResetTokens
                .Where(t => t.UsuarioId == usuarioId && !t.Usado)
                .ToListAsync(ct);

            if (tokensActivos.Any())
            {
                foreach (var token in tokensActivos)
                {
                    token.Usado = true; // Invalidar
                    token.FechaUso = DateTime.UtcNow; // Marcar como "revocado"
                }
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}

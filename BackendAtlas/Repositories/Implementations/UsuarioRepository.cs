using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============ QUERIES ============

        public async Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public async Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            // Búsqueda case-insensitive para emails
            return await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        // ============ COMMANDS ============

        public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default)
        {
            await _context.Usuarios.AddAsync(usuario, cancellationToken);
        }
    }
}

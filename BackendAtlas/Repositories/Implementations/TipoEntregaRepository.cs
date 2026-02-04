using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    public class TipoEntregaRepository : ITipoEntregaRepository
    {
        private readonly AppDbContext _context;

        public TipoEntregaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoEntrega>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.TiposEntrega
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<TipoEntrega?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.TiposEntrega
                .FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task ActualizarAsync(TipoEntrega tipoEntrega, CancellationToken cancellationToken = default)
        {
            _context.TiposEntrega.Update(tipoEntrega);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
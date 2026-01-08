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
    }
}
using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    public class MetodoPagoRepository : IMetodoPagoRepository
    {
        private readonly AppDbContext _context;

        public MetodoPagoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MetodoPago>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.MetodosPago
                .AsNoTracking()
                .Where(mp => mp.EsActivo)
                .ToListAsync(cancellationToken);
        }
    }
}
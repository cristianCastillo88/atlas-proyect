using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    /// <summary>
    /// Repositorio específico de Sucursal con métodos semánticos.
    /// </summary>
    public class SucursalRepository : ISucursalRepository
    {
        private readonly AppDbContext _context;

        public SucursalRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============ QUERIES ============

        public async Task<Sucursal?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Sucursales.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Sucursal?> ObtenerPorSlugConProductosAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Sucursales
                .AsNoTracking()
                .Include(s => s.Productos)
                 .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(s => s.Slug == slug && s.Activo, cancellationToken);
        }

        public async Task<IEnumerable<Sucursal>> ObtenerPorNegocioAsync(int negocioId, CancellationToken cancellationToken = default)
        {
            return await _context.Sucursales
                .AsNoTracking()
                .Where(s => s.NegocioId == negocioId)
                .OrderBy(s => s.Nombre)
                .ToListAsync(cancellationToken);
        }

        // ============ COMMANDS ============

        public async Task AgregarAsync(Sucursal sucursal, CancellationToken cancellationToken = default)
        {
            await _context.Sucursales.AddAsync(sucursal, cancellationToken);
        }

        public void Actualizar(Sucursal sucursal)
        {
            _context.Sucursales.Update(sucursal);
        }
    }
}
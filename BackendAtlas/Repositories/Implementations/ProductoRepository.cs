using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    /// <summary>
    /// Repositorio específico de Producto con métodos semánticos.
    /// OPTIMIZACIÓN: Todas las lecturas usan AsNoTracking() para máximo rendimiento.
    /// </summary>
    public class ProductoRepository : IProductoRepository
    {
        private readonly AppDbContext _context;

        public ProductoRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============ QUERIES (AsNoTracking para performance) ============

        public async Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // FindAsync es muy eficiente para búsquedas por PK
            return await _context.Productos.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Producto?> ObtenerPorIdConCategoriaAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Producto>> ObtenerCatalogoPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.SucursalId == sucursalId && p.Activo)
                .OrderBy(p => p.Categoria!.Nombre)
                .ThenBy(p => p.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Producto>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Productos
                .AsNoTracking()
                .Include(p => p.Categoria)
                .Where(p => p.Activo)
                .ToListAsync(cancellationToken);
        }

        // ============ COMMANDS (Sin SaveChanges - lo maneja UnitOfWork) ============

        public async Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default)
        {
            await _context.Productos.AddAsync(producto, cancellationToken);
        }

        public void Actualizar(Producto producto)
        {
            _context.Productos.Update(producto);
        }

        public void Eliminar(Producto producto)
        {
            _context.Productos.Remove(producto);
        }
    }
}

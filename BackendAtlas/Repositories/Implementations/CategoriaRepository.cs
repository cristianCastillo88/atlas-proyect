using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    /// <summary>
    /// Repositorio específico de Categoría con métodos semánticos.
    /// </summary>
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============ QUERIES ============

        public async Task<IEnumerable<Categoria>> ObtenerCategoriasActivasAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Categoria>> ObtenerCategoriasPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default)
        {
            return await _context.Categorias
                .AsNoTracking()
                .Where(c => c.SucursalId == sucursalId && c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Categorias
                .AnyAsync(c => c.Id == id, cancellationToken);
        }

        // ============ COMMANDS ============

        public async Task AgregarAsync(Categoria categoria, CancellationToken cancellationToken = default)
        {
            await _context.Categorias.AddAsync(categoria, cancellationToken);
        }
    }
}
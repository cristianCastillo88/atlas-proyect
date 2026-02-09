using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    public class NegocioRepository : INegocioRepository
    {
        private readonly AppDbContext _context;

        public NegocioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Negocio?> GetBySlugWithSucursalesAsync(string slug, CancellationToken cancellationToken = default)
        {
            return await _context.Negocios
                .Include(n => n.Sucursales)
                .FirstOrDefaultAsync(n => n.Slug == slug && n.Activo, cancellationToken);
        }

        public async Task<List<Negocio>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Negocios
                .Include(n => n.Sucursales)
                .Include(n => n.Usuarios)
                .ToListAsync(cancellationToken);
        }

        public async Task<Negocio?> GetByIdWithUsersAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Negocios
                .Include(n => n.Usuarios)
                .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
        }

        public async Task<Negocio?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Negocios.FindAsync(new object[] { id }, cancellationToken);
        }

        public void Actualizar(Negocio negocio)
        {
            _context.Negocios.Update(negocio);
        }

        public async Task AgregarAsync(Negocio negocio, CancellationToken cancellationToken = default)
        {
            await _context.Negocios.AddAsync(negocio, cancellationToken);
        }
    }
}

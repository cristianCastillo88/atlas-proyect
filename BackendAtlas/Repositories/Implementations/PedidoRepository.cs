using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Repositories.Implementations
{
    /// <summary>
    /// Repositorio específico de Pedido con métodos semánticos.
    /// </summary>
    public class PedidoRepository : IPedidoRepository
    {
        private readonly AppDbContext _context;

        public PedidoRepository(AppDbContext context)
        {
            _context = context;
        }

        // ============ QUERIES (AsNoTracking) ============

        public async Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Pedidos.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<Pedido?> ObtenerPorIdConDetallesAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.EstadoPedido)
                .Include(p => p.TipoEntrega)
                .Include(p => p.MetodoPago)
                .Include(p => p.DetallesPedido!)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Pedido>> ObtenerPedidosDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaInicio = fecha.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Pedidos
                .AsNoTracking()
                .Include(p => p.EstadoPedido)
                .Include(p => p.TipoEntrega)
                .Include(p => p.MetodoPago)
                .Include(p => p.DetallesPedido!)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.FechaCreacion >= fechaInicio && p.FechaCreacion < fechaFin)
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Pedido>> ObtenerPorSucursalYEstadoAsync(int sucursalId, string? estadoNombre, CancellationToken cancellationToken = default)
        {
            var query = _context.Pedidos
                .AsNoTracking()
                .Include(p => p.EstadoPedido)
                .Include(p => p.TipoEntrega)
                .Include(p => p.MetodoPago)
                .Include(p => p.DetallesPedido!)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.SucursalId == sucursalId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(estadoNombre))
            {
                query = query.Where(p => p.EstadoPedido!.Nombre == estadoNombre);
            }

            return await query
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync(cancellationToken);
        }

        // ============ COMMANDS ============

        public async Task AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default)
        {
            await _context.Pedidos.AddAsync(pedido, cancellationToken);
        }

        public void ActualizarEstado(Pedido pedido)
        {
            _context.Pedidos.Update(pedido);
        }
    }
}


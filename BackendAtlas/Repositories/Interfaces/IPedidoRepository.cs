using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio específico de Pedido con métodos semánticos.
    /// </summary>
    public interface IPedidoRepository
    {
        // ============ QUERIES ============
        Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Pedido?> ObtenerPorIdConDetallesAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Pedido>> ObtenerPedidosDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default);
        Task<IEnumerable<Pedido>> ObtenerPorSucursalYEstadoAsync(int sucursalId, string? estadoNombre, CancellationToken cancellationToken = default);
        
        // ============ COMMANDS ============
        Task AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default);
        void ActualizarEstado(Pedido pedido);
    }
}
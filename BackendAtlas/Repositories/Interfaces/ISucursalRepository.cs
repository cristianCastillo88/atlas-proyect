using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio específico de Sucursal con métodos semánticos.
    /// </summary>
    public interface ISucursalRepository
    {
        // ============ QUERIES ============
        Task<Sucursal?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Sucursal?> ObtenerPorSlugConProductosAsync(string slug, CancellationToken cancellationToken = default);
        Task<Sucursal?> ObtenerPorSlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<IEnumerable<Sucursal>> ObtenerPorNegocioAsync(int negocioId, CancellationToken cancellationToken = default);
        
        // ============ COMMANDS ============
        Task AgregarAsync(Sucursal sucursal, CancellationToken cancellationToken = default);
        void Actualizar(Sucursal sucursal);
    }
}
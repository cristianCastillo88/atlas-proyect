using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio específico de Producto con métodos semánticos de negocio.
    /// CQS: Para LECTURAS (AsNoTracking) - inyectar directamente en servicios.
    /// Para ESCRITURAS - acceder vía IUnitOfWork.
    /// </summary>
    public interface IProductoRepository
    {
        // ============ QUERIES (Métodos semánticos de lectura) ============
        Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Producto?> ObtenerPorIdConCategoriaAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Producto>> ObtenerCatalogoPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Producto>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default);
        
        // ============ COMMANDS (Métodos de escritura - usar vía UnitOfWork) ============
        Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
        void Actualizar(Producto producto);
        void Eliminar(Producto producto);
    }
}

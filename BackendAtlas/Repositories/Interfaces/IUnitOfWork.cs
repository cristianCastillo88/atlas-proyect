using Microsoft.EntityFrameworkCore.Storage;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work para coordinar operaciones de ESCRITURA transaccionales.
    /// CQS: Solo para Commands (Create, Update, Delete).
    /// Para Queries (Get, List), inyectar repositorios directamente.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // Repositorios (Para operaciones de ESCRITURA)
        IProductoRepository Productos { get; }
        IPedidoRepository Pedidos { get; }
        ICategoriaRepository Categorias { get; }
        ISucursalRepository Sucursales { get; }
        IUsuarioRepository Usuarios { get; }
        INegocioRepository Negocios { get; }

        // Persistencia - Guarda todos los cambios pendientes
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // Alias de CompleteAsync
        
        // Gestión de transacciones (Para operaciones complejas como Pedidos)
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}

using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio específico de Categoría con métodos semánticos.
    /// </summary>
    public interface ICategoriaRepository
    {
        // ============ QUERIES ============
        Task<IEnumerable<Categoria>> ObtenerCategoriasActivasAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Categoria>> ObtenerCategoriasPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default);
        Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);
        
        // ============ COMMANDS ============
        Task AgregarAsync(Categoria categoria, CancellationToken cancellationToken = default);
    }
}
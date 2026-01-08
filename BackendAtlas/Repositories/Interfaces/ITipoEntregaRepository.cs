using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio de Tipos de Entrega con métodos semánticos.
    /// </summary>
    public interface ITipoEntregaRepository
    {
        // ============ QUERIES ============
        Task<IEnumerable<TipoEntrega>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    }
}
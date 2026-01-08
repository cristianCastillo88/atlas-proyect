using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio de Métodos de Pago con métodos semánticos.
    /// </summary>
    public interface IMetodoPagoRepository
    {
        // ============ QUERIES ============
        Task<IEnumerable<MetodoPago>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default);
    }
}
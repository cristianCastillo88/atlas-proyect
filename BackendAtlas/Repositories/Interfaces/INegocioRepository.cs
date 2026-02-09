using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    public interface INegocioRepository
    {
        Task<Negocio?> GetBySlugWithSucursalesAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Negocio>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
        Task<Negocio?> GetByIdWithUsersAsync(int id, CancellationToken cancellationToken = default);
        
        // Standard methods needed for UnitOfWork pattern
        Task<Negocio?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        void Actualizar(Negocio negocio);
        Task AgregarAsync(Negocio negocio, CancellationToken cancellationToken = default);
    }
}

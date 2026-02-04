using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    public interface INegocioRepository
    {
        Task<Negocio?> GetBySlugWithSucursalesAsync(string slug, CancellationToken cancellationToken = default);
        Task<List<Negocio>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
        Task<Negocio?> GetByIdWithUsersAsync(int id, CancellationToken cancellationToken = default);
    }
}

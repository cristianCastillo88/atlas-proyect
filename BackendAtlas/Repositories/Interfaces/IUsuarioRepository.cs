using BackendAtlas.Domain;

namespace BackendAtlas.Repositories.Interfaces
{
    /// <summary>
    /// Repositorio de Usuario con métodos semánticos.
    /// </summary>
    public interface IUsuarioRepository
    {
        // ============ QUERIES ============
        Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Usuario?> ObtenerPorEmailAsync(string email, CancellationToken cancellationToken = default);
        
        Task<IEnumerable<Usuario>> ObtenerEmpleadosPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default);
        
        // ============ COMMANDS ============
        Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
        void Actualizar(Usuario usuario);
    }
}

using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface ISucursalService
    {
        Task<SucursalResponseDto> CrearSucursalAsync(SucursalCreateDto dto, CancellationToken cancellationToken = default);
        Task ModificarSucursalAsync(int id, SucursalUpdateDto dto, int usuarioId, string rol, CancellationToken cancellationToken = default);
        Task<IEnumerable<EmpleadoDto>> ObtenerEmpleadosAsync(int sucursalId, int solicintanteNegocioId, string rolSolicitante, int? sucursalIdSolicitante, CancellationToken cancellationToken = default);
        Task DarDeBajaEmpleadoAsync(int empleadoId, int solicitanteNegocioId, CancellationToken cancellationToken = default);
        Task<SucursalResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<SucursalResponseDto>> ObtenerPorNegocioAsync(int negocioId, CancellationToken cancellationToken = default);
        
        // Endpoint CON datos dinámicos (precio/stock) - mantener para compatibilidad
        Task<SucursalPublicaDto?> ObtenerPublicaPorSlugAsync(string slug, CancellationToken cancellationToken = default);
        
        // Nuevos métodos para separación de datos estáticos/dinámicos
        Task<SucursalPublicaEstaticaDto?> ObtenerPublicaEstaticaAsync(string slug, CancellationToken cancellationToken = default);
        Task<Dictionary<int, ProductoDinamicoDto>> ObtenerDatosDinamicosPorSlugAsync(string slug, CancellationToken cancellationToken = default);
        
        Task<bool> ToggleStatusAsync(int id, int userId, string userRole, CancellationToken cancellationToken = default);
    }
}
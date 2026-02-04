using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface ITenantService
    {
        Task RegistrarNuevoInquilinoAsync(CrearNegocioDto dto, CancellationToken cancellationToken = default);
        Task CrearEmpleadoAsync(CrearEmpleadoDto dto, int negocioIdDelAdmin, CancellationToken cancellationToken = default);
        Task<List<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
        Task<bool> ToggleTenantStatusAsync(int id, CancellationToken cancellationToken = default);
    }
}
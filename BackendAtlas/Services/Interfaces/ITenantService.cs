using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface ITenantService
    {
        Task RegistrarNuevoInquilinoAsync(CrearNegocioDto dto, CancellationToken cancellationToken = default);
        Task CrearEmpleadoAsync(CrearEmpleadoDto dto, int negocioIdDelAdmin, CancellationToken cancellationToken = default);
    }
}
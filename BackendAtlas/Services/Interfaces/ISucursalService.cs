using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface ISucursalService
    {
        Task<SucursalResponseDto> CrearSucursalAsync(SucursalCreateDto dto, CancellationToken cancellationToken = default);
        Task ModificarSucursalAsync(int id, SucursalUpdateDto dto, int usuarioId, string rol, CancellationToken cancellationToken = default);
    }
}
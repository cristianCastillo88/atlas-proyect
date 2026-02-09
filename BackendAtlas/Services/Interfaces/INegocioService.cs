using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface INegocioService
    {
        Task<NegocioPublicoDto?> GetNegocioPublicoAsync(string slug, CancellationToken cancellationToken = default);
        Task<NegocioBasicDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<NegocioBasicDto> ActualizarNegocioAsync(int id, NegocioUpdateDto dto, CancellationToken cancellationToken = default);
    }
}

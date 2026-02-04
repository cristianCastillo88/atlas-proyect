using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<CategoriaDto>> ObtenerPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default);
        Task<CategoriaDto> CrearCategoriaAsync(string nombre, int sucursalId, CancellationToken cancellationToken = default);
    }
}
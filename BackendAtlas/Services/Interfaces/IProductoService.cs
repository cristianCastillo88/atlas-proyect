using BackendAtlas.DTOs;
using BackendAtlas.DTOs.Common;

namespace BackendAtlas.Services.Interfaces
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoResponseDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ProductoResponseDto>> ObtenerPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default);
        
        // HIGH PRIORITY #4: Pagination
        Task<PagedResult<ProductoResponseDto>> ObtenerPorSucursalPaginadoAsync(
            int sucursalId, 
            int pageNumber = 1, 
            int pageSize = 20, 
            CancellationToken cancellationToken = default);
        Task<ProductoResponseDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
        Task<ProductoResponseDto> CrearAsync(ProductoCreateRequestDto request, CancellationToken cancellationToken = default);
        Task<ProductoResponseDto> ActualizarAsync(ProductoUpdateRequestDto request, CancellationToken cancellationToken = default);
        Task DesactivarProductoAsync(int id, CancellationToken cancellationToken = default);
    }
}
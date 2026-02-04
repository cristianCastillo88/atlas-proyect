using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoResponseDto> GenerarPedidoAsync(PedidoCreateDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<PedidoAdminListDto>> ObtenerPedidosGestionAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<PedidoAdminListDto>> ObtenerPedidosPorSucursalAsync(int sucursalId, string? estadoNombre, int? usuarioNegocioId, string? usuarioRol, int? usuarioSucursalId, CancellationToken cancellationToken = default);
        Task CambiarEstadoPedidoAsync(int id, CambiarEstadoDto dto, int? usuarioNegocioId, string? usuarioRol, int? usuarioSucursalId, CancellationToken cancellationToken = default);
    }
}
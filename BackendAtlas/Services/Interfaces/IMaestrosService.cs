using BackendAtlas.DTOs;

namespace BackendAtlas.Services.Interfaces
{
    public interface IMaestrosService
    {
        Task<IEnumerable<MetodoPagoDto>> ObtenerMetodosPagoActivosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<TipoEntregaDto>> ObtenerTiposEntregaAsync(CancellationToken cancellationToken = default);
    }
}
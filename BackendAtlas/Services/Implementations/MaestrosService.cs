using AutoMapper;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Servicio de Datos Maestros (Métodos de Pago, Tipos de Entrega).
    /// Solo requiere LECTURAS, por lo que inyecta repositorios directamente.
    /// </summary>
    public class MaestrosService : IMaestrosService
    {
        private readonly IMetodoPagoRepository _metodoPagoRepository;
        private readonly ITipoEntregaRepository _tipoEntregaRepository;
        private readonly IMapper _mapper;

        public MaestrosService(IMetodoPagoRepository metodoPagoRepository, ITipoEntregaRepository tipoEntregaRepository, IMapper mapper)
        {
            _metodoPagoRepository = metodoPagoRepository;
            _tipoEntregaRepository = tipoEntregaRepository;
            _mapper = mapper;
        }

        // ============ QUERIES (LECTURAS - AsNoTracking) ============

        public async Task<IEnumerable<MetodoPagoDto>> ObtenerMetodosPagoActivosAsync(CancellationToken cancellationToken = default)
        {
            // Repo directo para lectura - Método semántico
            var metodosPago = await _metodoPagoRepository.ObtenerTodosActivosAsync(cancellationToken);
            if (metodosPago == null)
            {
                return Enumerable.Empty<MetodoPagoDto>();
            }
            return _mapper.Map<IEnumerable<MetodoPagoDto>>(metodosPago);
        }

        public async Task<IEnumerable<TipoEntregaDto>> ObtenerTiposEntregaAsync(CancellationToken cancellationToken = default)
        {
            // Repo directo para lectura - Método semántico
            var tiposEntrega = await _tipoEntregaRepository.ObtenerTodosAsync(cancellationToken);
            if (tiposEntrega == null)
            {
                return Enumerable.Empty<TipoEntregaDto>();
            }
            return _mapper.Map<IEnumerable<TipoEntregaDto>>(tiposEntrega);
        }
    }
}
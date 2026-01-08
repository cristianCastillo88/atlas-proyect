using AutoMapper;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Servicio de Categorías aplicando CQS.
    /// - LECTURAS: ICategoriaRepository directo (AsNoTracking)
    /// - ESCRITURAS: IUnitOfWork
    /// </summary>
    public class CategoriaService : ICategoriaService
    {
        // CQS: Repo directo para LECTURAS
        private readonly ICategoriaRepository _categoriaRepository;
        
        // CQS: UnitOfWork para ESCRITURAS
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IMapper _mapper;

        public CategoriaService(
            ICategoriaRepository categoriaRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _categoriaRepository = categoriaRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ============ QUERIES (LECTURAS - AsNoTracking) ============

        public async Task<IEnumerable<CategoriaDto>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo para lectura rápida - Método semántico
            var categorias = await _categoriaRepository.ObtenerCategoriasActivasAsync(cancellationToken);
            if (categorias == null)
            {
                return Enumerable.Empty<CategoriaDto>();
            }
            return _mapper.Map<IEnumerable<CategoriaDto>>(categorias);
        }

        public async Task<CategoriaDto> ObtenerPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo - Método semántico
            var categorias = await _categoriaRepository.ObtenerCategoriasActivasAsync(cancellationToken);
            var categoria = categorias.FirstOrDefault(c => c.SucursalId == sucursalId);
            if (categoria == null)
            {
                throw new KeyNotFoundException($"No se encontraron categorías para la sucursal {sucursalId}");
            }
            return _mapper.Map<CategoriaDto>(categoria);
        }

        // ============ COMMANDS (ESCRITURAS - UnitOfWork) ============

        public async Task<CategoriaDto> CrearCategoriaAsync(string nombre, int sucursalId, CancellationToken cancellationToken = default)
        {
            var categoria = new Categoria
            {
                Nombre = nombre,
                SucursalId = sucursalId,
                Activa = true
            };
            
            // CQS: UnitOfWork para escritura
            await _unitOfWork.Categorias.AgregarAsync(categoria, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            
            return _mapper.Map<CategoriaDto>(categoria);
        }
    }
}
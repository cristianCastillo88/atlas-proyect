using AutoMapper;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Servicio de Productos aplicando CQS (Command Query Separation).
    /// - LECTURAS: IProductoRepository directo (AsNoTracking, rápido)
    /// - ESCRITURAS: IUnitOfWork (Transaccional, SaveChanges)
    /// </summary>
    public class ProductoService : IProductoService
    {
        // CQS: Repo directo para LECTURAS rápidas (AsNoTracking)
        private readonly IProductoRepository _productoRepository;
        
        // CQS: UnitOfWork solo para ESCRITURAS (Transaccional)
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IMapper _mapper;

        public ProductoService(
            IProductoRepository productoRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productoRepository = productoRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ============ QUERIES (LECTURAS - AsNoTracking) ============

        public async Task<IEnumerable<ProductoResponseDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo para lectura rápida
            var productos = await _productoRepository.ObtenerTodosActivosAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ProductoResponseDto>>(productos);
        }

        public async Task<IEnumerable<ProductoResponseDto>> ObtenerPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo con AsNoTracking - Método semántico de negocio
            var productos = await _productoRepository.ObtenerCatalogoPorSucursalAsync(sucursalId, cancellationToken);
            return _mapper.Map<IEnumerable<ProductoResponseDto>>(productos);
        }

        public async Task<ProductoResponseDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo para consulta - Método semántico
            var producto = await _productoRepository.ObtenerPorIdConCategoriaAsync(id, cancellationToken);
            if (producto == null)
            {
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
            }
            return _mapper.Map<ProductoResponseDto>(producto);
        }

        // ============ COMMANDS (ESCRITURAS - UnitOfWork) ============

        public async Task<ProductoResponseDto> CrearAsync(ProductoCreateRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.Precio <= 0)
            {
                throw new ArgumentException("El precio debe ser mayor a 0.");
            }

            // Validar categoría (Repo directo para lectura)
            var categoriaExiste = await _unitOfWork.Categorias.ExisteAsync(request.CategoriaId, cancellationToken);
            if (!categoriaExiste)
            {
                throw new ArgumentException($"La categoría con ID {request.CategoriaId} no existe.");
            }

            var producto = _mapper.Map<Producto>(request);
            producto.Activo = true;

            // CQS: UnitOfWork para escritura transaccional
            await _unitOfWork.Productos.AgregarAsync(producto, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            // Recargar con repo directo para devolver DTO
            var productoCreado = await _productoRepository.ObtenerPorIdConCategoriaAsync(producto.Id, cancellationToken);
            return _mapper.Map<ProductoResponseDto>(productoCreado);
        }

        public async Task<ProductoResponseDto> ActualizarAsync(ProductoUpdateRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request.Precio <= 0)
            {
                throw new ArgumentException("El precio debe ser mayor a 0.");
            }

            var categoriaExiste = await _unitOfWork.Categorias.ExisteAsync(request.CategoriaId, cancellationToken);
            if (!categoriaExiste)
            {
                throw new ArgumentException($"La categoría con ID {request.CategoriaId} no existe.");
            }

            // Para actualizar necesitamos tracking, usamos UnitOfWork
            var productoExistente = await _unitOfWork.Productos.ObtenerPorIdAsync(request.Id, cancellationToken);
            if (productoExistente == null)
            {
                throw new KeyNotFoundException($"Producto con ID {request.Id} no encontrado.");
            }

            _mapper.Map(request, productoExistente);
            
            // CQS: UnitOfWork para escritura
            _unitOfWork.Productos.Actualizar(productoExistente);
            await _unitOfWork.CompleteAsync(cancellationToken);

            // Recargar con repo directo
            var productoActualizado = await _productoRepository.ObtenerPorIdConCategoriaAsync(request.Id, cancellationToken);
            return _mapper.Map<ProductoResponseDto>(productoActualizado);
        }

        public async Task DesactivarProductoAsync(int id, CancellationToken cancellationToken = default)
        {
            // Para actualizar necesitamos tracking
            var producto = await _unitOfWork.Productos.ObtenerPorIdAsync(id, cancellationToken);
            if (producto == null)
            {
                throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
            }

            producto.Activo = false;
            
            // CQS: UnitOfWork para escritura
            _unitOfWork.Productos.Actualizar(producto);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}
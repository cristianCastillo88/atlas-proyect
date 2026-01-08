using AutoMapper;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Servicio de Pedidos aplicando CQS.
    /// - LECTURAS: IPedidoRepository directo (AsNoTracking)
    /// - ESCRITURAS: IUnitOfWork con transacciones
    /// </summary>
    public class PedidoService : IPedidoService
    {
        // CQS: Repo directo para LECTURAS
        private readonly IPedidoRepository _pedidoRepository;
        
        // CQS: UnitOfWork para ESCRITURAS transaccionales
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IMapper _mapper;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ============ COMMAND: Generar Pedido (ESCRITURA Transaccional) ============

        public async Task<PedidoResponseDto> GenerarPedidoAsync(PedidoCreateDto dto, CancellationToken cancellationToken = default)
        {
            // Iniciar transacción para operación compleja
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var pedido = new Pedido
                {
                    NombreCliente = dto.NombreCliente,
                    DireccionCliente = string.IsNullOrWhiteSpace(dto.DireccionCliente) ? null : dto.DireccionCliente,
                    TelefonoCliente = dto.TelefonoCliente,
                    MetodoPagoId = dto.MetodoPagoId,
                    TipoEntregaId = dto.TipoEntregaId,
                    SucursalId = dto.SucursalId,
                    Observaciones = dto.Observaciones,
                    FechaCreacion = DateTime.Now,
                    EstadoPedidoId = 1,
                    Total = 0
                };

                var detalles = new List<DetallePedido>();
                foreach (var item in dto.Items)
                {
                    // Obtener producto CON tracking (necesitamos modificarlo)
                    var producto = await _unitOfWork.Productos.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
                    if (producto == null || !producto.Activo)
                    {
                        throw new ArgumentException($"Producto con ID {item.ProductoId} no existe o no está activo.");
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        throw new ArgumentException($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}");
                    }

                    // Restar stock
                    producto.Stock -= item.Cantidad;
                    _unitOfWork.Productos.Actualizar(producto);

                    var detalle = new DetallePedido
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio
                    };

                    pedido.Total += detalle.Cantidad * detalle.PrecioUnitario;
                    detalles.Add(detalle);
                }

                pedido.DetallesPedido = detalles;

                // Guardar pedido
                await _unitOfWork.Pedidos.AgregarAsync(pedido, cancellationToken);

                // Commit: Guarda pedido + productos actualizados ATÓMICAMENTE
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new PedidoResponseDto
                {
                    Id = pedido.Id,
                    Total = pedido.Total,
                    FechaCreacion = pedido.FechaCreacion,
                    EstadoPedido = "Pendiente"
                };
            }
            catch
            {
                // Rollback: Si falla, NO se resta stock ni se crea pedido
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        // ============ QUERIES (LECTURAS - AsNoTracking) ============

        public async Task<IEnumerable<PedidoAdminListDto>> ObtenerPedidosGestionAsync(CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo para lectura rápida - Método semántico
            var pedidos = await _pedidoRepository.ObtenerPedidosDelDiaAsync(DateTime.Today, cancellationToken);
            return _mapper.Map<IEnumerable<PedidoAdminListDto>>(pedidos);
        }

        public async Task<IEnumerable<PedidoAdminListDto>> ObtenerPedidosPorSucursalAsync(int sucursalId, string? estadoNombre, CancellationToken cancellationToken = default)
        {
            // CQS: Repo directo - Método semántico
            var pedidos = await _pedidoRepository.ObtenerPorSucursalYEstadoAsync(sucursalId, estadoNombre, cancellationToken);
            return _mapper.Map<IEnumerable<PedidoAdminListDto>>(pedidos);
        }

        // ============ COMMAND: Cambiar Estado (ESCRITURA) ============

        public async Task CambiarEstadoPedidoAsync(int id, CambiarEstadoDto dto, CancellationToken cancellationToken = default)
        {
            // Obtener pedido con tracking para actualizar
            var pedido = await _unitOfWork.Pedidos.ObtenerPorIdAsync(id, cancellationToken);
            if (pedido == null)
            {
                throw new KeyNotFoundException($"Pedido con ID {id} no encontrado.");
            }

            // Actualizar estado
            pedido.EstadoPedidoId = dto.NuevoEstadoId;
            
            // CQS: UnitOfWork para escritura - Método semántico
            _unitOfWork.Pedidos.ActualizarEstado(pedido);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}
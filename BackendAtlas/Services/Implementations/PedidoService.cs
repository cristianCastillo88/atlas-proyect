using AutoMapper;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

using Microsoft.AspNetCore.SignalR;
using BackendAtlas.Hubs;
using BackendAtlas.Extensions;

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
        private readonly ISucursalRepository _sucursalRepository;
        
        // CQS: UnitOfWork para ESCRITURAS transaccionales
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IMapper _mapper;
        private readonly IHubContext<PedidosHub> _hubContext;

        public PedidoService(
            IPedidoRepository pedidoRepository,
            ISucursalRepository sucursalRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHubContext<PedidosHub> hubContext)
        {
            _pedidoRepository = pedidoRepository;
            _sucursalRepository = sucursalRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        // ============ COMMAND: Generar Pedido (ESCRITURA Transaccional) ============

        public async Task<PedidoResponseDto> GenerarPedidoAsync(PedidoCreateDto dto, CancellationToken cancellationToken = default)
        {
            // Iniciar transacción para operación compleja
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // SANITIZACIÓN: Limpiar inputs del usuario antes de guardar en BD
                var pedido = new Pedido
                {
                    NombreCliente = dto.NombreCliente.SanitizeHtml()?.NormalizeWhitespace() ?? dto.NombreCliente,
                    DireccionCliente = string.IsNullOrWhiteSpace(dto.DireccionCliente) 
                        ? null 
                        : dto.DireccionCliente.SanitizeHtml()?.NormalizeWhitespace(),
                    TelefonoCliente = dto.TelefonoCliente.Trim(),
                    MetodoPagoId = dto.MetodoPagoId,
                    TipoEntregaId = dto.TipoEntregaId,
                    SucursalId = dto.SucursalId,
                    Observaciones = dto.Observaciones.SanitizeHtml()?.NormalizeWhitespace(),
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
                        PrecioUnitario = producto.Precio,
                        Aclaraciones = item.Aclaraciones.SanitizeHtml()?.NormalizeWhitespace()
                    };

                    pedido.Total += detalle.Cantidad * detalle.PrecioUnitario;
                    detalles.Add(detalle);
                }

                if (dto.TipoEntregaId == 2) // ID 2 = Delivery
                {
                    // Obtener configuración de sucursal para saber costo
                    var sucursal = await _unitOfWork.Sucursales.ObtenerPorIdAsync(dto.SucursalId, cancellationToken);
                    if (sucursal != null)
                    {
                        pedido.Total += sucursal.PrecioDelivery;
                    }
                    // Si no existe sucursal o no tiene precio, asume 0 o precio base global si quisiéramos mantener backward compat (pero optamos por propiedad nueva)
                }

                pedido.DetallesPedido = detalles;

                // Guardar pedido
                await _unitOfWork.Pedidos.AgregarAsync(pedido, cancellationToken);

                // Commit: Guarda pedido + productos actualizados ATÓMICAMENTE
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                // ============ SIGNALR NOTIFICATION ============
                try
                {
                    await _hubContext.Clients.Group(dto.SucursalId.ToString())
                        .SendAsync("NuevoPedido", new
                        {
                            Id = pedido.Id,
                            NombreCliente = pedido.NombreCliente,
                            Total = pedido.Total,
                            Fecha = pedido.FechaCreacion,
                            Estado = "Pendiente"
                        }, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the order
                    Console.WriteLine($"Error sending SignalR notification: {ex.Message}");
                }

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

        public async Task<IEnumerable<PedidoAdminListDto>> ObtenerPedidosPorSucursalAsync(int sucursalId, string? estadoNombre, int? usuarioNegocioId, string? usuarioRol, int? usuarioSucursalId, CancellationToken cancellationToken = default)
        {
            // Validación de seguridad (Lectura -> Repository)
            if (usuarioRol == "AdminNegocio")
            {
                if (!usuarioNegocioId.HasValue)
                     throw new UnauthorizedAccessException("Negocio ID no encontrado.");

                var suc = await _sucursalRepository.ObtenerPorIdAsync(sucursalId, cancellationToken);
                if (suc == null || suc.NegocioId != usuarioNegocioId.Value)
                    throw new UnauthorizedAccessException("No tienes permisos sobre esta sucursal.");
            }
            else if (usuarioRol == "Empleado")
            {
                if (!usuarioSucursalId.HasValue || usuarioSucursalId.Value != sucursalId)
                    throw new UnauthorizedAccessException("No tienes permisos sobre esta sucursal.");
            }

            // CQS: Repo directo - Método semántico
            var pedidos = await _pedidoRepository.ObtenerPorSucursalYEstadoAsync(sucursalId, estadoNombre, cancellationToken);
            return _mapper.Map<IEnumerable<PedidoAdminListDto>>(pedidos);
        }

        // ============ COMMAND: Cambiar Estado (ESCRITURA) ============

        public async Task CambiarEstadoPedidoAsync(int id, CambiarEstadoDto dto, int? usuarioNegocioId, string? usuarioRol, int? usuarioSucursalId, CancellationToken cancellationToken = default)
        {
            // Obtener pedido con tracking para actualizar
            var pedido = await _unitOfWork.Pedidos.ObtenerPorIdAsync(id, cancellationToken);
            if (pedido == null)
            {
                throw new KeyNotFoundException($"Pedido con ID {id} no encontrado.");
            }

            // Seguridad: validar permissions
            if (usuarioRol == "AdminNegocio")
            {
                 if (!usuarioNegocioId.HasValue)
                     throw new UnauthorizedAccessException("Negocio ID no encontrado.");

                 // Check sucursal ownership via UoW (consistent read for write op)
                 var suc = await _unitOfWork.Sucursales.ObtenerPorIdAsync(pedido.SucursalId, cancellationToken);
                  if (suc == null || suc.NegocioId != usuarioNegocioId.Value)
                        throw new UnauthorizedAccessException("No tienes permisos sobre esta sucursal.");
            }
            else if (usuarioRol == "Empleado")
            {
                if (!usuarioSucursalId.HasValue || usuarioSucursalId.Value != pedido.SucursalId)
                     throw new UnauthorizedAccessException("No tienes permisos sobre esta sucursal.");
            }

            // Actualizar estado
            pedido.EstadoPedidoId = dto.NuevoEstadoId;
            
            // CQS: UnitOfWork para escritura - Método semántico
            _unitOfWork.Pedidos.ActualizarEstado(pedido);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}
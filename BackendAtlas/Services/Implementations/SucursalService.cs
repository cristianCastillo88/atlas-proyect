using AutoMapper;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    /// <summary>
    /// Servicio de Sucursales aplicando CQS.
    /// - LECTURAS: ISucursalRepository directo (AsNoTracking)
    /// - ESCRITURAS: IUnitOfWork
    /// </summary>
    public class SucursalService : ISucursalService
    {
        // CQS: Repo directo para LECTURAS
        private readonly ISucursalRepository _sucursalRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMetodoPagoRepository _metodoPagoRepository;
        private readonly ITipoEntregaRepository _tipoEntregaRepository;
        private readonly IProductoRepository _productoRepository; // Para datos dinámicos
        
        // CQS: UnitOfWork para ESCRITURAS
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly IMapper _mapper;

        public SucursalService(
            ISucursalRepository sucursalRepository,
            IUsuarioRepository usuarioRepository,
            IMetodoPagoRepository metodoPagoRepository,
            ITipoEntregaRepository tipoEntregaRepository,
            IProductoRepository productoRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _sucursalRepository = sucursalRepository;
            _usuarioRepository = usuarioRepository;
            _metodoPagoRepository = metodoPagoRepository;
            _tipoEntregaRepository = tipoEntregaRepository;
            _productoRepository = productoRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ============ COMMANDS (ESCRITURAS - UnitOfWork) ============

        public async Task<SucursalResponseDto> CrearSucursalAsync(SucursalCreateDto dto, CancellationToken cancellationToken = default)
        {
            // Generar Slug
            var slug = GenerateSlug(dto.Nombre);

            var sucursal = new Sucursal
            {
                NegocioId = dto.NegocioId,
                Nombre = dto.Nombre,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                Slug = slug
            };

            // CQS: UnitOfWork para escritura
            await _unitOfWork.Sucursales.AgregarAsync(sucursal, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            // Los métodos de pago y tipos de entrega son globales, no se crean por sucursal

            return new SucursalResponseDto
            {
                Id = sucursal.Id,
                NegocioId = sucursal.NegocioId,
                Nombre = sucursal.Nombre,
                Slug = sucursal.Slug,
                Direccion = sucursal.Direccion
            };
        }

        public async Task ModificarSucursalAsync(int id, SucursalUpdateDto dto, int usuarioId, string rol, CancellationToken cancellationToken = default)
        {
            // Para actualizar necesitamos tracking, usamos UnitOfWork
            var sucursal = await _unitOfWork.Sucursales.ObtenerPorIdAsync(id, cancellationToken);
            if (sucursal == null)
            {
                throw new KeyNotFoundException("Sucursal no encontrada.");
            }

            // Validación de seguridad
            if (rol == "AdminNegocio")
            {
                var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId, cancellationToken);
                if (usuario?.NegocioId != sucursal.NegocioId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para modificar esta sucursal.");
                }
            }
            // SuperAdmin puede modificar cualquiera

            sucursal.Direccion = dto.Direccion;
            sucursal.Telefono = dto.Telefono;
            sucursal.Horario = dto.Horario;
            sucursal.UrlInstagram = dto.UrlInstagram;
            sucursal.UrlFacebook = dto.UrlFacebook;
            sucursal.PrecioDelivery = dto.PrecioDelivery;

            // CQS: UnitOfWork para escritura
            _unitOfWork.Sucursales.Actualizar(sucursal);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        private string GenerateSlug(string nombre)
        {
            return nombre.ToLower().Replace(" ", "-").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
        }

        public async Task<IEnumerable<EmpleadoDto>> ObtenerEmpleadosAsync(int sucursalId, int solicintanteNegocioId, string rolSolicitante, int? sucursalIdSolicitante, CancellationToken cancellationToken = default)
        {
            // Validar permisos
            if (rolSolicitante == "AdminNegocio")
            {
                // Verificar que la sucursal pertenece al negocio del admin
                var sucursal = await _sucursalRepository.ObtenerPorIdAsync(sucursalId, cancellationToken);
                if (sucursal == null || sucursal.NegocioId != solicintanteNegocioId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos sobre esta sucursal.");
                }
            }
            else if (rolSolicitante == "Empleado")
            {
                if (!sucursalIdSolicitante.HasValue || sucursalIdSolicitante.Value != sucursalId)
                {
                    throw new UnauthorizedAccessException("Solo puedes ver empleados de tu sucursal.");
                }
            }
            
            // CQS: LECTURA directa via Repo
            var empleados = await _usuarioRepository.ObtenerEmpleadosPorSucursalAsync(sucursalId, cancellationToken);
            return _mapper.Map<IEnumerable<EmpleadoDto>>(empleados);
        }

        public async Task DarDeBajaEmpleadoAsync(int empleadoId, int solicitanteNegocioId, CancellationToken cancellationToken = default)
        {
            // Obtener empleado (necesitamos la entidad para modificar, podríamos usar UoW para leer también si vamos a modificar)
            // Aquí usamos UoW porque es parte de una transacción de "Escritura/Lógica de negocio" que termina en SaveChanges
            var empleado = await _unitOfWork.Usuarios.ObtenerPorIdAsync(empleadoId, cancellationToken);
            
            if (empleado == null)
            {
                throw new KeyNotFoundException("Empleado no encontrado.");
            }

            if (empleado.Rol != RolUsuario.Empleado)
            {
                throw new InvalidOperationException("Solo se pueden dar de baja empleados.");
            }

            // Verificar que la sucursal del empleado pertenece al negocio del solicitante
            if (empleado.SucursalId.HasValue)
            {
                // Podemos usar el repo de lectura para chequear la sucursal rapidito, o UoW. 
                // Usaremos UoW para consistencia en la operacion.
                var sucursal = await _unitOfWork.Sucursales.ObtenerPorIdAsync(empleado.SucursalId.Value, cancellationToken);
                if (sucursal == null || sucursal.NegocioId != solicitanteNegocioId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para dar de baja este empleado.");
                }
            }
            else 
            {
                // Si es empleado y no tiene sucursal (raro), igual verificamos negocioId si lo tiene el usuario directo?
                // El modelo Usuario tiene NegocioId? Si.
                if (empleado.NegocioId != solicitanteNegocioId)
                {
                     throw new UnauthorizedAccessException("No tienes permisos para dar de baja este empleado.");
                }
            }

            empleado.Activo = false;
            
            // El usuario ya está trackeado por _unitOfWork.Usuarios.ObtenerPorIdAsync (si el repo devuelve trackeado)
            // Revisando UsuarioRepository: ObtenerPorIdAsync usa AsNoTracking(). 
            // !! IMPORTANTE: Si el repo usa AsNoTracking, tenemos que hacer Attach o Update.
            
            _unitOfWork.Usuarios.Actualizar(empleado); // Asumimos que existe este método genérico o específico
            // Check IUsuarioRepository -> I MUST ADD Update method.
            
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        public async Task<SucursalResponseDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (sucursal == null) return null;
            
            return new SucursalResponseDto
            {
                Id = sucursal.Id,
                NegocioId = sucursal.NegocioId,
                Nombre = sucursal.Nombre,
                Slug = sucursal.Slug,
                Direccion = sucursal.Direccion,
                Telefono = sucursal.Telefono,
                Horario = sucursal.Horario,
                UrlInstagram = sucursal.UrlInstagram,
                UrlFacebook = sucursal.UrlFacebook,
                Activo = sucursal.Activo,
                PrecioDelivery = sucursal.PrecioDelivery
            };
        }

        public async Task<IEnumerable<SucursalResponseDto>> ObtenerPorNegocioAsync(int negocioId, CancellationToken cancellationToken = default)
        {
            var sucursales = await _sucursalRepository.ObtenerPorNegocioAsync(negocioId, cancellationToken);
            return sucursales.Select(s => new SucursalResponseDto
            {
                Id = s.Id,
                NegocioId = s.NegocioId,
                Nombre = s.Nombre,
                Slug = s.Slug,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Horario = s.Horario,
                UrlInstagram = s.UrlInstagram,
                UrlFacebook = s.UrlFacebook,
                Activo = s.Activo,
                PrecioDelivery = s.PrecioDelivery
            });
        }

        public async Task<SucursalPublicaDto?> ObtenerPublicaPorSlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var sucursal = await _sucursalRepository.ObtenerPorSlugConProductosAsync(slug, cancellationToken);
            if (sucursal == null) return null;

             // Los métodos de pago y tipos de entrega son globales
            var metodosPago = await _metodoPagoRepository.ObtenerTodosActivosAsync(cancellationToken);
            var tiposEntrega = await _tipoEntregaRepository.ObtenerTodosAsync(cancellationToken);

            return new SucursalPublicaDto
            {
                Id = sucursal.Id,
                Nombre = sucursal.Nombre,
                Direccion = sucursal.Direccion,
                Telefono = sucursal.Telefono,
                Slug = sucursal.Slug,
                Horario = sucursal.Horario,
                UrlInstagram = sucursal.UrlInstagram,
                UrlFacebook = sucursal.UrlFacebook,
                Productos = sucursal.Productos?.Where(p => p.Activo).Select(p => new ProductoPublicoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    UrlImagen = p.UrlImagen,
                    Stock = p.Stock,
                    CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría"
                }).ToList() ?? new List<ProductoPublicoDto>(),
                MetodosPago = metodosPago.Select(m => new MetodoPagoDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre
                }).ToList(),
                TiposEntrega = tiposEntrega.Select(t => new TipoEntregaDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                }).ToList(),
                PrecioDelivery = sucursal.PrecioDelivery
            };
        }

        /// <summary>
        /// Obtener sucursal pública con info ESTÁTICA solamente (cacheable 5-10 min)
        /// </summary>
        public async Task<SucursalPublicaEstaticaDto?> ObtenerPublicaEstaticaAsync(string slug, CancellationToken cancellationToken = default)
        {
            var sucursal = await _sucursalRepository.ObtenerPorSlugConProductosAsync(slug, cancellationToken);
            if (sucursal == null) return null;

            var metodosPago = await _metodoPagoRepository.ObtenerTodosActivosAsync(cancellationToken);
            var tiposEntrega = await _tipoEntregaRepository.ObtenerTodosAsync(cancellationToken);

            return new SucursalPublicaEstaticaDto
            {
                Id = sucursal.Id,
                Nombre = sucursal.Nombre,
                Direccion = sucursal.Direccion,
                Telefono = sucursal.Telefono,
                Slug = sucursal.Slug,
                Horario = sucursal.Horario,
                UrlInstagram = sucursal.UrlInstagram,
                UrlFacebook = sucursal.UrlFacebook,
                // Solo info ESTÁTICA de productos (sin precio ni stock)
                Productos = sucursal.Productos?.Where(p => p.Activo).Select(p => new ProductoPublicoEstaticaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    UrlImagen = p.UrlImagen,
                    CategoriaNombre = p.Categoria?.Nombre ?? "Sin categoría"
                }).ToList() ?? new List<ProductoPublicoEstaticaDto>(),
                MetodosPago = metodosPago.Select(m => new MetodoPagoDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre
                }).ToList(),
                TiposEntrega = tiposEntrega.Select(t => new TipoEntregaDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre
                }).ToList(),
                PrecioDelivery = sucursal.PrecioDelivery
            };
        }

        /// <summary>
        /// Obtener datos DINÁMICOS (precio + stock) en tiempo real (sin cache)
        /// </summary>
        public async Task<Dictionary<int, ProductoDinamicoDto>> ObtenerDatosDinamicosPorSlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            // Primero obtener la sucursal para validar que existe
            var sucursal = await _sucursalRepository.ObtenerPorSlugConProductosAsync(slug, cancellationToken);
            if (sucursal == null)
            {
                return new Dictionary<int, ProductoDinamicoDto>();
            }

            // Query ultra-eficiente: solo precio y stock
            var datosDinamicos = await _productoRepository.ObtenerDatosDinamicosPorSucursalAsync(sucursal.Id, cancellationToken);

            // Mapear a DTO
            return datosDinamicos.ToDictionary(
                kvp => kvp.Key,
                kvp => new ProductoDinamicoDto
                {
                    Id = kvp.Key,
                    Precio = kvp.Value.Precio,
                    Stock = kvp.Value.Stock
                });
        }

        public async Task<bool> ToggleStatusAsync(int id, int userId, string userRole, CancellationToken cancellationToken = default)
        {
            var sucursal = await _unitOfWork.Sucursales.ObtenerPorIdAsync(id, cancellationToken);
            if (sucursal == null) return false;

            // Validación de seguridad
            if (userRole == "AdminNegocio")
            {
                var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(userId, cancellationToken);
                if (usuario?.NegocioId != sucursal.NegocioId)
                {
                    throw new UnauthorizedAccessException("No tienes permisos para actualizar esta sucursal.");
                }
            }

            sucursal.Activo = !sucursal.Activo;
            _unitOfWork.Sucursales.Actualizar(sucursal);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return sucursal.Activo;
        }
    }
}
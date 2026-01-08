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
        
        // CQS: UnitOfWork para ESCRITURAS
        private readonly IUnitOfWork _unitOfWork;

        public SucursalService(
            ISucursalRepository sucursalRepository,
            IUnitOfWork unitOfWork)
        {
            _sucursalRepository = sucursalRepository;
            _unitOfWork = unitOfWork;
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

            // CQS: UnitOfWork para escritura
            _unitOfWork.Sucursales.Actualizar(sucursal);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        private string GenerateSlug(string nombre)
        {
            return nombre.ToLower().Replace(" ", "-").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
        }
    }
}
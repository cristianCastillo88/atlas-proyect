using BackendAtlas.Data;
using BackendAtlas.Domain;
using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Services.Implementations
{
    public class TenantService : ITenantService
    {
        private readonly AppDbContext _context;
        private readonly INegocioRepository _negocioRepository;

        public TenantService(AppDbContext context, INegocioRepository negocioRepository)
        {
            _context = context;
            _negocioRepository = negocioRepository;
        }

        public async Task RegistrarNuevoInquilinoAsync(CrearNegocioDto dto, CancellationToken cancellationToken = default)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Crear Negocio
                var negocioSlug = GenerarSlug(dto.NombreNegocio);
                var negocio = new Negocio
                {
                    Nombre = dto.NombreNegocio,
                    Slug = negocioSlug,
                    FechaRegistro = DateTime.UtcNow
                };
                _context.Negocios.Add(negocio);
                await _context.SaveChangesAsync(cancellationToken);

                // Derivar nombre de calle (sin número) desde la dirección
                var direccion = string.IsNullOrEmpty(dto.DireccionCentral) ? dto.DireccionSucursalPrincipal : dto.DireccionCentral;
                var calleSinNumero = ExtraerCalleSinNumero(direccion);
                var nombreSucursal = (dto.NombreNegocio + calleSinNumero.Replace(" ", "")).Trim();
                var slug = GenerarSlug(dto.NombreNegocio) + "-" + GenerarSlug(calleSinNumero);

                // Crear Sucursal Principal con nombre derivado y teléfono provisto
                var sucursal = new Sucursal
                {
                    NegocioId = negocio.Id,
                    Nombre = nombreSucursal,
                    Direccion = direccion,
                    Telefono = dto.Telefono,
                    Slug = slug
                };
                _context.Sucursales.Add(sucursal);
                await _context.SaveChangesAsync(cancellationToken);

                // Crear Usuario AdminNegocio
                var usuario = new Usuario
                {
                    Email = dto.DatosDueno.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.DatosDueno.Password),
                    Nombre = dto.DatosDueno.Nombre,
                    NegocioId = negocio.Id,
                    Rol = RolUsuario.AdminNegocio,
                    Activo = true, // ACTIVAR INMEDIATAMENTE
                    FechaCreacion = DateTime.UtcNow,
                    RequiereCambioPassword = false
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private static string ExtraerCalleSinNumero(string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion)) return string.Empty;
            direccion = direccion.Trim();
            // Tomar parte antes del primer dígito
            var idx = direccion.IndexOfAny("0123456789".ToCharArray());
            var calle = idx > 0 ? direccion.Substring(0, idx) : direccion;
            return calle.Trim();
        }

        private static string GenerarSlug(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var lower = texto.ToLowerInvariant();
            // Reemplazar espacios por guiones y eliminar caracteres no alfanuméricos excepto guiones
            var sb = new System.Text.StringBuilder();
            foreach (var ch in lower)
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(ch);
                else if (char.IsWhiteSpace(ch)) sb.Append('-');
                else if (ch == '-' || ch == '_') sb.Append(ch);
            }
            // Compactar múltiples guiones
            var slug = sb.ToString();
            while (slug.Contains("--")) slug = slug.Replace("--", "-");
            return slug.Trim('-');
        }

        public async Task CrearEmpleadoAsync(CrearEmpleadoDto dto, int negocioIdDelAdmin, CancellationToken cancellationToken = default)
        {
            // Validar que SucursalId pertenece al negocioIdDelAdmin
            var sucursal = await _context.Sucursales.FirstOrDefaultAsync(s => s.Id == dto.SucursalId && s.NegocioId == negocioIdDelAdmin, cancellationToken);
            if (sucursal == null)
            {
                throw new UnauthorizedAccessException("La sucursal no pertenece a tu negocio.");
            }

            var usuario = new Usuario
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Nombre = dto.Nombre,
                NegocioId = negocioIdDelAdmin,
                SucursalId = dto.SucursalId,
                Rol = RolUsuario.Empleado,
                Activo = true, // ACTIVAR INMEDIATAMENTE
                FechaCreacion = DateTime.UtcNow,
                RequiereCambioPassword = false
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync(cancellationToken);
        }


        public async Task<List<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
        {
            var negocios = await _negocioRepository.GetAllWithDetailsAsync(cancellationToken);

            return negocios.Select(n => new TenantDto
            {
                Id = n.Id,
                Nombre = n.Nombre,
                Slug = n.Slug,
                DueñoEmail = n.Usuarios?.FirstOrDefault(u => u.Rol == RolUsuario.AdminNegocio)?.Email ?? "N/A",
                CantidadSucursales = n.Sucursales?.Count ?? 0,
                Activo = n.Activo,
                FechaRegistro = n.FechaRegistro
            }).ToList();
        }

        public async Task<bool> ToggleTenantStatusAsync(int id, CancellationToken cancellationToken = default)
        {
            var negocio = await _negocioRepository.GetByIdWithUsersAsync(id, cancellationToken);
            if (negocio == null) return false;

            // Invertir estado del negocio
            negocio.Activo = !negocio.Activo;

            // Invertir estado de todos los usuarios del negocio
            var usuariosNegocio = negocio.Usuarios?.Where(u => u.NegocioId == id).ToList();
            if (usuariosNegocio != null)
            {
                foreach (var usuario in usuariosNegocio)
                {
                    usuario.Activo = negocio.Activo;
                }
            }
            
            // Assuming _context tracks this since we got it via repository using the same context instance implicitly?
            // Actually Repos use same context if scoped.
            _context.Negocios.Update(negocio);
            await _context.SaveChangesAsync(cancellationToken);
            
            return negocio.Activo;
        }
    }
}
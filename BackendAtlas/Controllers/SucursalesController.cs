using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackendAtlas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SucursalesController : ControllerBase
    {
        private readonly ISucursalService _sucursalService;
        private readonly ISucursalRepository _sucursalRepository;
        private readonly BackendAtlas.Data.AppDbContext _context;

        public SucursalesController(ISucursalService sucursalService, ISucursalRepository sucursalRepository, BackendAtlas.Data.AppDbContext context)
        {
            _sucursalService = sucursalService;
            _sucursalRepository = sucursalRepository;
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CrearSucursal([FromBody] SucursalCreateDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _sucursalService.CrearSucursalAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetSucursal), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin, AdminNegocio")]
        public async Task<IActionResult> ModificarSucursal(int id, [FromBody] SucursalUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Usuario no identificado.");
            }

            await _sucursalService.ModificarSucursalAsync(id, dto, userId, roleClaim ?? "", cancellationToken);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSucursal(int id, CancellationToken cancellationToken = default)
        {
            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (sucursal == null)
            {
                return NotFound();
            }

            var response = new SucursalResponseDto
            {
                Id = sucursal.Id,
                NegocioId = sucursal.NegocioId,
                Nombre = sucursal.Nombre,
                Slug = sucursal.Slug,
                Direccion = sucursal.Direccion,
                Activo = sucursal.Activo
            };

            return Ok(response);
        }

        [HttpGet("negocios/{negocioId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSucursalesPorNegocio(int negocioId, CancellationToken cancellationToken = default)
        {
            var sucursales = await _sucursalRepository.ObtenerPorNegocioAsync(negocioId, cancellationToken);
            var response = sucursales.Select(s => new SucursalResponseDto
            {
                Id = s.Id,
                NegocioId = s.NegocioId,
                Nombre = s.Nombre,
                Slug = s.Slug,
                Direccion = s.Direccion,
                Activo = s.Activo
            });

            return Ok(response);
        }

        [HttpGet("public/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSucursalPublica(string slug, CancellationToken cancellationToken = default)
        {
            var sucursal = await _sucursalRepository.ObtenerPorSlugConProductosAsync(slug, cancellationToken);
            if (sucursal == null)
            {
                return NotFound("Sucursal no encontrada");
            }

            // Los métodos de pago y tipos de entrega son globales
            var metodosPago = await _context.MetodosPago
                .Where(m => m.EsActivo)
                .ToListAsync(cancellationToken);
            var tiposEntrega = await _context.TiposEntrega
                .ToListAsync(cancellationToken);

            var response = new SucursalPublicaDto
            {
                Id = sucursal.Id,
                Nombre = sucursal.Nombre,
                Direccion = sucursal.Direccion,
                Telefono = sucursal.Telefono,
                Slug = sucursal.Slug,
                Horario = sucursal.Horario,
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
                }).ToList()
            };

            return Ok(response);
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "SuperAdmin, AdminNegocio")]
        public async Task<IActionResult> ToggleSucursalStatus(int id, CancellationToken cancellationToken = default)
        {
            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(id, cancellationToken);
            if (sucursal == null)
            {
                return NotFound("Sucursal no encontrada.");
            }

            // Validación de seguridad
            var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (roleClaim == "AdminNegocio")
            {
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Usuario no identificado.");
                }
                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario?.NegocioId != sucursal.NegocioId)
                {
                    return Unauthorized("No tienes permisos para actualizar esta sucursal.");
                }
            }

            sucursal.Activo = !sucursal.Activo;
            _sucursalRepository.Actualizar(sucursal);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { message = $"Estado de la sucursal actualizado a: {(sucursal.Activo ? "Activa" : "Inactiva")}" });
        }
    }
}
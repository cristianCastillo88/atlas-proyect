using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BackendAtlas.Data;
using BackendAtlas.Domain;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ITenantService _tenantService;
       private readonly AppDbContext _context;

       public AdminController(ITenantService tenantService, AppDbContext context)
        {
            _tenantService = tenantService;
           _context = context;
        }

        [HttpPost("tenants/registrar-negocio")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RegistrarNegocio([FromBody] CrearNegocioDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                if (dto == null)
                    return BadRequest("El DTO no puede ser nulo");
                
                if (string.IsNullOrEmpty(dto.NombreNegocio))
                    return BadRequest("El nombre del negocio es requerido");
                
                if (dto.DatosDueno == null)
                    return BadRequest("Los datos del dueño son requeridos");

                var direccion = string.IsNullOrWhiteSpace(dto.DireccionCentral) ? dto.DireccionSucursalPrincipal : dto.DireccionCentral;
                if (string.IsNullOrWhiteSpace(direccion))
                    return BadRequest("La dirección es requerida");

                if (string.IsNullOrWhiteSpace(dto.Telefono))
                    return BadRequest("El número de teléfono es requerido");

                await _tenantService.RegistrarNuevoInquilinoAsync(dto, cancellationToken);
                return Ok(new { message = "Negocio registrado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
            }
        }

       [HttpGet("tenants")]
       [Authorize(Roles = "SuperAdmin")]
       public async Task<IActionResult> GetAllTenants(CancellationToken cancellationToken = default)
       {
           var negocios = await _context.Negocios
               .Include(n => n.Sucursales)
               .Include(n => n.Usuarios)
               .ToListAsync(cancellationToken);

           var tenantsDto = negocios.Select(n => new TenantDto
           {
               Id = n.Id,
               Nombre = n.Nombre,
               DueñoEmail = n.Usuarios?.FirstOrDefault(u => u.Rol == RolUsuario.AdminNegocio)?.Email ?? "N/A",
               CantidadSucursales = n.Sucursales?.Count ?? 0,
               Activo = n.Activo,
               FechaRegistro = n.FechaRegistro
           }).ToList();

           return Ok(tenantsDto);
       }

       [HttpPatch("tenants/{id}/toggle-status")]
       public async Task<IActionResult> ToggleTenantStatus(int id, CancellationToken cancellationToken = default)
       {
           var negocio = await _context.Negocios.Include(n => n.Usuarios).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
           if (negocio == null)
           {
               return NotFound("Negocio no encontrado.");
           }

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

           _context.Negocios.Update(negocio);
           await _context.SaveChangesAsync(cancellationToken);

           return Ok(new { message = $"Estado del negocio actualizado a: {(negocio.Activo ? "Activo" : "Inactivo")}" });
       }
    }
}
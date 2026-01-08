using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAtlas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminNegocio")]
    public class EmpleadosController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly BackendAtlas.Data.AppDbContext _context;

        public EmpleadosController(ITenantService tenantService, BackendAtlas.Data.AppDbContext context)
        {
            _tenantService = tenantService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CrearEmpleado([FromBody] CrearEmpleadoDto dto, CancellationToken cancellationToken = default)
        {
            var negocioIdClaim = User.FindFirst("negocioId")?.Value;
            if (string.IsNullOrEmpty(negocioIdClaim) || !int.TryParse(negocioIdClaim, out var negocioId))
            {
                return Unauthorized("Negocio ID no encontrado.");
            }

            await _tenantService.CrearEmpleadoAsync(dto, negocioId, cancellationToken);
            return Ok("Empleado creado exitosamente.");
        }

        [HttpGet("sucursal/{sucursalId}")]
        [Authorize(Roles = "AdminNegocio,SuperAdmin,Empleado")]
        public async Task<IActionResult> ListarPorSucursal(int sucursalId, CancellationToken cancellationToken = default)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Debug: Log all claims
            var allClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("=== CLAIMS DEBUG ===");
            foreach (var claim in allClaims)
            {
                Console.WriteLine(claim);
            }
            Console.WriteLine("====================");
            
            if (role == "AdminNegocio")
            {
                var negocioIdClaim = User.FindFirst("negocioId")?.Value;
                if (string.IsNullOrEmpty(negocioIdClaim) || !int.TryParse(negocioIdClaim, out var negocioId))
                {
                    return Unauthorized("Negocio ID no encontrado.");
                }

                var pertenece = await _context.Sucursales.FindAsync(new object[] { sucursalId }, cancellationToken);
                if (pertenece == null || pertenece.NegocioId != negocioId)
                {
                    return Unauthorized("No tienes permisos sobre esta sucursal.");
                }
            }
            else if (role == "Empleado")
            {
                var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
                Console.WriteLine($"Empleado SucursalId claim: {sucursalIdClaim}");
                
                if (string.IsNullOrEmpty(sucursalIdClaim) || !int.TryParse(sucursalIdClaim, out var empleadoSucursalId))
                {
                    return Unauthorized($"Sucursal ID no encontrado en token. Claims disponibles: {string.Join(", ", allClaims)}");
                }

                if (empleadoSucursalId != sucursalId)
                {
                    return Unauthorized("Solo puedes ver empleados de tu sucursal.");
                }
            }

            var empleados = _context.Usuarios
                .Where(u => u.SucursalId == sucursalId && u.Rol == Domain.RolUsuario.Empleado && u.Activo)
                .Select(u => new { u.Id, u.Nombre, u.Email, u.SucursalId })
                .ToList();

            return Ok(empleados);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DarDeBajaEmpleado(int id, CancellationToken cancellationToken = default)
        {
            var negocioIdClaim = User.FindFirst("negocioId")?.Value;
            if (string.IsNullOrEmpty(negocioIdClaim) || !int.TryParse(negocioIdClaim, out var negocioId))
            {
                return Unauthorized("Negocio ID no encontrado.");
            }

            var empleado = await _context.Usuarios.FindAsync(new object[] { id }, cancellationToken);
            if (empleado == null)
            {
                return NotFound("Empleado no encontrado.");
            }

            if (empleado.Rol != Domain.RolUsuario.Empleado)
            {
                return BadRequest("Solo se pueden dar de baja empleados.");
            }

            // Verificar que la sucursal del empleado pertenece al negocio
            if (empleado.SucursalId.HasValue)
            {
                var sucursal = await _context.Sucursales.FindAsync(new object[] { empleado.SucursalId.Value }, cancellationToken);
                if (sucursal == null || sucursal.NegocioId != negocioId)
                {
                    return Unauthorized("No tienes permisos para dar de baja este empleado.");
                }
            }

            empleado.Activo = false;
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new { mensaje = "Empleado dado de baja exitosamente" });
        }
    }
}
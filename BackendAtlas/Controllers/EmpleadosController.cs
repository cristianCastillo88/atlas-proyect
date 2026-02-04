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
        private readonly ISucursalService _sucursalService;

        public EmpleadosController(ITenantService tenantService, ISucursalService sucursalService)
        {
            _tenantService = tenantService;
            _sucursalService = sucursalService;
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
            var rol = User.FindFirst(ClaimTypes.Role)?.Value;
            var negocioIdClaim = User.FindFirst("negocioId")?.Value;
            int.TryParse(negocioIdClaim ?? "0", out var negocioId);
            
            var sucursalIdClaim = User.FindFirst("SucursalId")?.Value;
            int.TryParse(sucursalIdClaim ?? "0", out var sucursalIdUsuario);
            int? sucursalIdNullable = sucursalIdUsuario == 0 ? null : sucursalIdUsuario;

            try 
            {
                var empleados = await _sucursalService.ObtenerEmpleadosAsync(sucursalId, negocioId, rol, sucursalIdNullable, cancellationToken);
                return Ok(empleados);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DarDeBajaEmpleado(int id, CancellationToken cancellationToken = default)
        {
            var negocioIdClaim = User.FindFirst("negocioId")?.Value;
            if (string.IsNullOrEmpty(negocioIdClaim) || !int.TryParse(negocioIdClaim, out var negocioId))
            {
                return Unauthorized("Negocio ID no encontrado.");
            }

            try 
            {
                await _sucursalService.DarDeBajaEmpleadoAsync(id, negocioId, cancellationToken);
                return Ok(new { mensaje = "Empleado dado de baja exitosamente" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
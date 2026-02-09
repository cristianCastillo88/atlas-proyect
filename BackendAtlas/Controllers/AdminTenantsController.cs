using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAtlas.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminTenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly INegocioService _negocioService;

        public AdminTenantsController(ITenantService tenantService, INegocioService negocioService)
        {
            _tenantService = tenantService;
            _negocioService = negocioService;
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
            var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
            return Ok(tenants);
        }

        [HttpPut("tenants/{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ActualizarNegocio(int id, [FromBody] NegocioUpdateDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var resultado = await _negocioService.ActualizarNegocioAsync(id, dto, cancellationToken);
                return Ok(resultado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Negocio no encontrado");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("tenants/{id}/toggle-status")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ToggleTenantStatus(int id, CancellationToken cancellationToken = default)
        {
            var result = await _tenantService.ToggleTenantStatusAsync(id, cancellationToken);
            
            // Note: Currently result being false is ambiguous (Not Found vs set to Inactive). 
            // Ideally service should return nullable or throw if not found.
            // For now, we assume success if no exception.

            return Ok(new { message = $"Estado del negocio actualizado." });
             
        }
    }
}
using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendAtlas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;

        public PedidosController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<PedidoResponseDto>> Post(PedidoCreateDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var pedido = await _pedidoService.GenerarPedidoAsync(request, cancellationToken);
                return CreatedAtAction(nameof(Post), new { id = pedido.Id }, pedido);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "Sin excepción interna";
                return StatusCode(500, $"Error interno del servidor: {ex.Message}. Inner: {innerMessage}");
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<IEnumerable<PedidoAdminListDto>>> GetDashboard(CancellationToken cancellationToken = default)
        {
            var pedidos = await _pedidoService.ObtenerPedidosGestionAsync(cancellationToken);
            return Ok(pedidos);
        }

        [HttpGet("sucursal/{sucursalId}")]
        [Authorize(Roles = "AdminNegocio,Empleado,SuperAdmin")]
        public async Task<ActionResult<IEnumerable<PedidoAdminListDto>>> GetPorSucursal(int sucursalId, [FromQuery] string? estado = null, CancellationToken cancellationToken = default)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            // Búsqueda resiliente de claims (case-insensitive)
            var negocioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Equals("negocioId", StringComparison.OrdinalIgnoreCase))?.Value;
            int.TryParse(negocioIdClaim ?? "0", out var negocioId);
            int? negocioIdNullable = negocioId == 0 ? null : negocioId;

            var sucursalIdClaim = User.Claims.FirstOrDefault(c => c.Type.Equals("sucursalId", StringComparison.OrdinalIgnoreCase))?.Value;
            int.TryParse(sucursalIdClaim ?? "0", out var usuarioSucursalId);
            int? usuarioSucursalIdNullable = usuarioSucursalId == 0 ? null : usuarioSucursalId;

            try
            {
                var pedidos = await _pedidoService.ObtenerPedidosPorSucursalAsync(sucursalId, estado, negocioIdNullable, role, usuarioSucursalIdNullable, cancellationToken);
                return Ok(pedidos);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "AdminNegocio,Empleado,SuperAdmin")]
        public async Task<IActionResult> PatchEstado(int id, CambiarEstadoDto request, CancellationToken cancellationToken = default)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var negocioIdClaim = User.Claims.FirstOrDefault(c => c.Type.Equals("negocioId", StringComparison.OrdinalIgnoreCase))?.Value;
            int.TryParse(negocioIdClaim ?? "0", out var negocioId);
            int? negocioIdNullable = negocioId == 0 ? null : negocioId;

            var sucursalIdClaim = User.Claims.FirstOrDefault(c => c.Type.Equals("sucursalId", StringComparison.OrdinalIgnoreCase))?.Value;
            int.TryParse(sucursalIdClaim ?? "0", out var usuarioSucursalId);
            int? usuarioSucursalIdNullable = usuarioSucursalId == 0 ? null : usuarioSucursalId;

            try
            {
                await _pedidoService.CambiarEstadoPedidoAsync(id, request, negocioIdNullable, role, usuarioSucursalIdNullable, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }
    }
}
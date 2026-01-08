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
        private readonly BackendAtlas.Data.AppDbContext _context;

        public PedidosController(IPedidoService pedidoService, BackendAtlas.Data.AppDbContext context)
        {
            _pedidoService = pedidoService;
            _context = context;
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

            if (role == "AdminNegocio")
            {
                var negocioIdClaim = User.FindFirst("negocioId")?.Value;
                if (string.IsNullOrEmpty(negocioIdClaim) || !int.TryParse(negocioIdClaim, out var negocioId))
                    return Unauthorized("Negocio ID no encontrado.");

                var suc = await _context.Sucursales.FindAsync(sucursalId);
                if (suc == null || suc.NegocioId != negocioId)
                    return Unauthorized("No tienes permisos sobre esta sucursal.");
            }
            else if (role == "Empleado")
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                    return Unauthorized("Usuario no identificado.");

                var usuario = await _context.Usuarios.FindAsync(userId);
                if (usuario?.SucursalId != sucursalId)
                    return Unauthorized("No tienes permisos sobre esta sucursal.");
            }

            var pedidos = await _pedidoService.ObtenerPedidosPorSucursalAsync(sucursalId, estado, cancellationToken);
            return Ok(pedidos);
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "AdminNegocio,Empleado,SuperAdmin")]
        public async Task<IActionResult> PatchEstado(int id, CambiarEstadoDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                // Seguridad: validar que el usuario tenga permisos sobre la sucursal del pedido
                var pedidoDb = await _context.Pedidos.FindAsync(id);
                if (pedidoDb == null) return NotFound("Pedido no encontrado.");

                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                if (role == "AdminNegocio")
                {
                    var negocioIdClaim = User.FindFirst("negocioId")?.Value;
                    if (string.IsNullOrEmpty(negocioIdClaim) || !int.TryParse(negocioIdClaim, out var negocioId))
                        return Unauthorized("Negocio ID no encontrado.");
                    var suc = await _context.Sucursales.FindAsync(pedidoDb.SucursalId);
                    if (suc == null || suc.NegocioId != negocioId)
                        return Unauthorized("No tienes permisos sobre esta sucursal.");
                }
                else if (role == "Empleado")
                {
                    var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                        return Unauthorized("Usuario no identificado.");
                    var usuario = await _context.Usuarios.FindAsync(userId);
                    if (usuario?.SucursalId != pedidoDb.SucursalId)
                        return Unauthorized("No tienes permisos sobre esta sucursal.");
                }

                await _pedidoService.CambiarEstadoPedidoAsync(id, request, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno del servidor: " + ex.Message);
            }
        }
    }
}
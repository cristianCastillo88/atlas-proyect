using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAtlas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaestrosController : ControllerBase
    {
        private readonly IMaestrosService _maestrosService;

        public MaestrosController(IMaestrosService maestrosService)
        {
            _maestrosService = maestrosService;
        }

        [HttpGet("pagos")]
        public async Task<ActionResult<IEnumerable<MetodoPagoDto>>> GetMetodosPago(CancellationToken cancellationToken = default)
        {
            var metodosPago = await _maestrosService.ObtenerMetodosPagoActivosAsync(cancellationToken);
            return Ok(metodosPago);
        }

        [HttpGet("entregas")]
        public async Task<ActionResult<IEnumerable<TipoEntregaDto>>> GetTiposEntrega(CancellationToken cancellationToken = default)
        {
            var tiposEntrega = await _maestrosService.ObtenerTiposEntregaAsync(cancellationToken);
            return Ok(tiposEntrega);
        }
    }
}
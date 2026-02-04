using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAtlas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NegociosController : ControllerBase
    {
        private readonly INegocioService _negocioService;

        public NegociosController(INegocioService negocioService)
        {
            _negocioService = negocioService;
        }

        /// <summary>
        /// Get public business information by slug - Cached for 5 minutes
        /// </summary>
        [HttpGet("public/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "slug" })] // HIGH PRIORITY #3: Cache 5 min
        public async Task<IActionResult> GetNegocioPublico(string slug, CancellationToken cancellationToken = default)
        {
            var negocio = await _negocioService.GetNegocioPublicoAsync(slug, cancellationToken);

            if (negocio == null)
            {
                return NotFound("Negocio no encontrado");
            }

            return Ok(negocio);
        }
    }
}

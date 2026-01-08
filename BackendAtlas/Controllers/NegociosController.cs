using BackendAtlas.DTOs;
using BackendAtlas.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NegociosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NegociosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("public/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNegocioPublico(string slug, CancellationToken cancellationToken = default)
        {
            var negocio = await _context.Negocios
                .Include(n => n.Sucursales)
                .FirstOrDefaultAsync(n => n.Slug == slug && n.Activo, cancellationToken);

            if (negocio == null)
            {
                return NotFound("Negocio no encontrado");
            }

            var response = new NegocioPublicoDto
            {
                Id = negocio.Id,
                Nombre = negocio.Nombre,
                Slug = negocio.Slug,
                UrlLogo = negocio.UrlLogo,
                Sucursales = negocio.Sucursales?
                    .Where(s => s.Activo)
                    .Select(s => new SucursalResumenDto
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Telefono = s.Telefono,
                        Slug = s.Slug
                    })
                    .ToList() ?? new List<SucursalResumenDto>()
            };

            return Ok(response);
        }
    }
}

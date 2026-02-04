using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendAtlas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriasController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> Get(CancellationToken cancellationToken = default)
        {
            var categorias = await _categoriaService.ObtenerTodosActivosAsync(cancellationToken);
            return Ok(categorias);
        }

        [HttpGet("sucursal/{sucursalId}")]
        public async Task<ActionResult<IEnumerable<CategoriaDto>>> GetBySucursal(int sucursalId, CancellationToken cancellationToken = default)
        {
            var categorias = await _categoriaService.ObtenerPorSucursalAsync(sucursalId, cancellationToken);
            return Ok(categorias);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaDto>> Create([FromBody] CategoriaDto dto, CancellationToken cancellationToken = default)
        {
            var result = await _categoriaService.CrearCategoriaAsync(dto.Nombre, dto.SucursalId, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
    }
}
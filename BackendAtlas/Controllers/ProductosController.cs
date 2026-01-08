using BackendAtlas.DTOs;
using BackendAtlas.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendAtlas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;

        public ProductosController(IProductoService productoService)
        {
            _productoService = productoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> Get(CancellationToken cancellationToken = default)
        {
            var productos = await _productoService.ObtenerTodosAsync(cancellationToken);
            return Ok(productos);
        }

        [HttpGet("sucursal/{sucursalId}")]
        public async Task<ActionResult<IEnumerable<ProductoResponseDto>>> GetBySucursal(int sucursalId, CancellationToken cancellationToken = default)
        {
            var productos = await _productoService.ObtenerPorSucursalAsync(sucursalId, cancellationToken);
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoResponseDto>> Get(int id, CancellationToken cancellationToken = default)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id, cancellationToken);
            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult<ProductoResponseDto>> Post(ProductoCreateRequestDto request, CancellationToken cancellationToken = default)
        {
            var producto = await _productoService.CrearAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = producto.Id }, producto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductoResponseDto>> Put(int id, ProductoUpdateRequestDto request, CancellationToken cancellationToken = default)
        {
            if (id != request.Id)
            {
                return BadRequest("El ID del producto no coincide.");
            }

            var producto = await _productoService.ActualizarAsync(request, cancellationToken);
            return Ok(producto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
        {
            await _productoService.DesactivarProductoAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
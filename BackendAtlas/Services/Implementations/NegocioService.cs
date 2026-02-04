using BackendAtlas.DTOs;
using BackendAtlas.Repositories.Interfaces;
using BackendAtlas.Services.Interfaces;

namespace BackendAtlas.Services.Implementations
{
    public class NegocioService : INegocioService
    {
        private readonly INegocioRepository _negocioRepository;

        public NegocioService(INegocioRepository negocioRepository)
        {
            _negocioRepository = negocioRepository;
        }

        public async Task<NegocioBasicDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var negocio = await _negocioRepository.GetByIdWithUsersAsync(id, cancellationToken);
            if (negocio == null) return null;

            return new NegocioBasicDto
            {
                Id = negocio.Id,
                Nombre = negocio.Nombre,
                Slug = negocio.Slug ?? string.Empty
            };
        }

        public async Task<NegocioPublicoDto?> GetNegocioPublicoAsync(string slug, CancellationToken cancellationToken = default)
        {
            var negocio = await _negocioRepository.GetBySlugWithSucursalesAsync(slug, cancellationToken);


            if (negocio == null)
            {
                return null;
            }

            // Manual mapping to ensure exact structure as requested
            return new NegocioPublicoDto
            {
                Id = negocio.Id,
                Nombre = negocio.Nombre,
                Slug = negocio.Slug ?? string.Empty, // Should be present if we found it by slug
                UrlLogo = negocio.UrlLogo,
                Sucursales = negocio.Sucursales?
                    .Where(s => s.Activo)
                    .Select(s => new SucursalResumenDto
                    {
                        Id = s.Id,
                        Nombre = s.Nombre,
                        Direccion = s.Direccion,
                        Telefono = s.Telefono,
                        Slug = s.Slug,
                        Horario = s.Horario
                    })
                    .ToList() ?? new List<SucursalResumenDto>()
            };
        }
    }
}

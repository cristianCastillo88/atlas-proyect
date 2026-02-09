using BackendAtlas.DTOs;
using BackendAtlas.Domain;
using BackendAtlas.Services.Implementations;
using BackendAtlas.Repositories.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace BackendAtlas.UnitTests.Services
{
    public class NegocioServiceTests
    {
        private readonly Mock<INegocioRepository> _negocioRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly NegocioService _negocioService;

        public NegocioServiceTests()
        {
            _negocioRepositoryMock = new Mock<INegocioRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            
            // Setup unitOfWork.Negocios -> _negocioRepositoryMock
            // Esto es crucial porque mi servicio hace: _unitOfWork.Negocios.ObtenerPorIdAsync
            _unitOfWorkMock.Setup(u => u.Negocios).Returns(_negocioRepositoryMock.Object);

            _negocioService = new NegocioService(_negocioRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ActualizarNegocioAsync_ConDatosValidos_ActualizaYRetornaDto()
        {
            // Arrange
            var id = 1;
            var dto = new NegocioUpdateDto { Nombre = "Nuevo Nombre", Slug = "nuevo-slug" };
            var negocio = new Negocio { Id = id, Nombre = "Viejo", Slug = "viejo", Sucursales = new List<Sucursal>() };

            _negocioRepositoryMock.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(negocio);

            // Simulamos que NO existe otro negocio con ese slug
            _negocioRepositoryMock.Setup(r => r.GetBySlugWithSucursalesAsync(dto.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Negocio?)null);

            // Act
            var result = await _negocioService.ActualizarNegocioAsync(id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Nombre.Should().Be(dto.Nombre);
            result.Slug.Should().Be(dto.Slug);
            
            // Verificamos que se llamó a Actualizar y CompleteAsync
            _negocioRepositoryMock.Verify(r => r.Actualizar(negocio), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ActualizarNegocioAsync_ConSlugDuplicado_LanzaInvalidOperationException()
        {
             // Arrange
            var id = 1;
            var dto = new NegocioUpdateDto { Nombre = "Nuevo", Slug = "slug-existente" };
            var negocio = new Negocio { Id = id, Nombre = "Viejo" };
            var otroNegocio = new Negocio { Id = 2, Nombre = "Otro", Slug = "slug-existente" }; // ID diferente = Conflicto

            _negocioRepositoryMock.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(negocio);
            
            _negocioRepositoryMock.Setup(r => r.GetBySlugWithSucursalesAsync(dto.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(otroNegocio);

            // Act
            var act = async () => await _negocioService.ActualizarNegocioAsync(id, dto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*slug*");
            
            // Verificamos que NUNCA se llamó a CompleteAsync
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ActualizarNegocioAsync_ConMismoSlug_NoLanzaExcepcion()
        {
             // Arrange
            var id = 1;
            var dto = new NegocioUpdateDto { Nombre = "Nuevo", Slug = "mi-slug-actual" };
            var negocio = new Negocio { Id = id, Nombre = "Viejo", Slug = "mi-slug-actual" };

            _negocioRepositoryMock.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(negocio);
            
            // El repo devuelve el MISMO negocio cuando buscamos por slug
            _negocioRepositoryMock.Setup(r => r.GetBySlugWithSucursalesAsync(dto.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(negocio);

            // Act
            var result = await _negocioService.ActualizarNegocioAsync(id, dto);

            // Assert
            result.Should().NotBeNull();
            result.Slug.Should().Be("mi-slug-actual");
            
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

using AutoMapper;
using BackendAtlas.DTOs;
using BackendAtlas.Domain;
using BackendAtlas.Services.Implementations;
using BackendAtlas.Repositories.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace BackendAtlas.UnitTests.Services
{
    public class SucursalServiceTests
    {
        private readonly Mock<ISucursalRepository> _sucursalRepoMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
        private readonly Mock<IMetodoPagoRepository> _metodoPagoRepoMock;
        private readonly Mock<ITipoEntregaRepository> _tipoEntregaRepoMock;
        private readonly Mock<IProductoRepository> _productoRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMapper> _mapperMock;
        
        private readonly SucursalService _sucursalService;

        public SucursalServiceTests()
        {
            _sucursalRepoMock = new Mock<ISucursalRepository>();
            _usuarioRepoMock = new Mock<IUsuarioRepository>();
            _metodoPagoRepoMock = new Mock<IMetodoPagoRepository>();
            _tipoEntregaRepoMock = new Mock<ITipoEntregaRepository>();
            _productoRepoMock = new Mock<IProductoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapperMock = new Mock<IMapper>();

            // Setup crítico: UnitOfWork.Sucursales devuelve nuestro mock de repo
            _unitOfWorkMock.Setup(u => u.Sucursales).Returns(_sucursalRepoMock.Object);
            // Tambien necesitamos UoW.Usuarios para verificaciones de permisos
            _unitOfWorkMock.Setup(u => u.Usuarios).Returns(_usuarioRepoMock.Object);

            _sucursalService = new SucursalService(
                _sucursalRepoMock.Object,
                _usuarioRepoMock.Object,
                _metodoPagoRepoMock.Object,
                _tipoEntregaRepoMock.Object,
                _productoRepoMock.Object,
                _unitOfWorkMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task ModificarSucursalAsync_SuperAdmin_ActualizaCorrectamente()
        {
            // Arrange
            var id = 10;
            var userId = 99;
            var rol = "SuperAdmin";
            
            var dto = new SucursalUpdateDto 
            { 
                Nombre = "Sucursal Nueva", 
                Slug = "sucursal-nueva",
                Direccion = "Calle Falsa 123",
                Telefono = "555-1234"
            };

            var sucursal = new Sucursal { Id = id, Nombre = "Vieja", Slug = "vieja", Direccion = "Old", Telefono = "Old", Activo = true };

            // Mock ObtenerPorId (via UoW)
            _sucursalRepoMock.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(sucursal);

            // Mock slug unico
            _sucursalRepoMock.Setup(r => r.ObtenerPorSlugConProductosAsync(dto.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Sucursal?)null);

            // Act
            await _sucursalService.ModificarSucursalAsync(id, dto, userId, rol);

            // Assert
            sucursal.Nombre.Should().Be(dto.Nombre);
            sucursal.Slug.Should().Be(dto.Slug);
            sucursal.Direccion.Should().Be(dto.Direccion);

            _sucursalRepoMock.Verify(r => r.Actualizar(sucursal), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ModificarSucursalAsync_SlugDuplicado_LanzaExcepcion()
        {
            // Arrange
            var id = 10;
            var dto = new SucursalUpdateDto 
            { 
                Nombre = "Test", 
                Slug = "slug-existente", // Conflicto
                Direccion = "Test", 
                Telefono = "123" 
            };
            
            
            var sucursal = new Sucursal { Id = id, Nombre = "Test", Slug = "mi-slug", Direccion = "Test", Telefono = "123" };
            var otraSucursal = new Sucursal { Id = 11, Nombre = "Otra", Slug = "slug-existente", Direccion = "Otra", Telefono = "456" };

            _sucursalRepoMock.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(sucursal);
            
            // Mock conflicto
            _sucursalRepoMock.Setup(r => r.ObtenerPorSlugConProductosAsync(dto.Slug, It.IsAny<CancellationToken>()))
                .ReturnsAsync(otraSucursal);

            // Act
            var act = async () => await _sucursalService.ModificarSucursalAsync(id, dto, 1, "SuperAdmin");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*slug*");
            _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ModificarSucursalAsync_AdminNegocioAjeno_LanzaUnauthorized()
        {
            // Arrange
            var id = 10;
            var userId = 5;
            var negocioIdPropio = 100;
            var negocioIdAjeno = 200;
            
            var dto = new SucursalUpdateDto { Nombre = "X", Slug = "x", Direccion = "X", Telefono = "X" };
            var sucursal = new Sucursal { Id = id, NegocioId = negocioIdAjeno, Nombre = "S", Slug = "s", Direccion = "D", Telefono = "T" }; // Pertenece a otro negocio
            var usuario = new Usuario 
            { 
                Id = userId, 
                NegocioId = negocioIdPropio,
                Nombre = "User",
                Email = "user@test.com",
                PasswordHash = "hash"
            };  

            _sucursalRepoMock.Setup(r => r.ObtenerPorIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(sucursal);
            _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

            // Act
            var act = async () => await _sucursalService.ModificarSucursalAsync(id, dto, userId, "AdminNegocio");

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}

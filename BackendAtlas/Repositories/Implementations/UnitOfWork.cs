using BackendAtlas.Data;
using BackendAtlas.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace BackendAtlas.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        // Lazy initialization de repositorios
        private IProductoRepository? _productos;
        private IPedidoRepository? _pedidos;
        private ICategoriaRepository? _categorias;
        private ISucursalRepository? _sucursales;
        private IUsuarioRepository? _usuarios;
        private INegocioRepository? _negocios;

        public UnitOfWork(
            AppDbContext context,
            IProductoRepository productoRepository,
            IPedidoRepository pedidoRepository,
            ICategoriaRepository categoriaRepository,
            ISucursalRepository sucursalRepository,
            IUsuarioRepository usuarioRepository,
            INegocioRepository negocioRepository)
        {
            _context = context;
            _productos = productoRepository;
            _pedidos = pedidoRepository;
            _categorias = categoriaRepository;
            _sucursales = sucursalRepository;
            _usuarios = usuarioRepository;
            _negocios = negocioRepository;
        }

        // Propiedades de repositorios
        public IProductoRepository Productos 
        {
            get
            {
                if (_productos == null)
                    throw new InvalidOperationException("ProductoRepository not initialized");
                return _productos;
            }
        }

        public IPedidoRepository Pedidos 
        {
            get
            {
                if (_pedidos == null)
                    throw new InvalidOperationException("PedidoRepository not initialized");
                return _pedidos;
            }
        }

        public ICategoriaRepository Categorias 
        {
            get
            {
                if (_categorias == null)
                    throw new InvalidOperationException("CategoriaRepository not initialized");
                return _categorias;
            }
        }

        public ISucursalRepository Sucursales 
        {
            get
            {
                if (_sucursales == null)
                    throw new InvalidOperationException("SucursalRepository not initialized");
                return _sucursales;
            }
        }

        public IUsuarioRepository Usuarios 
        {
            get
            {
                if (_usuarios == null)
                    throw new InvalidOperationException("UsuarioRepository not initialized");
                return _usuarios;
            }
        }

        public INegocioRepository Negocios 
        {
            get
            {
                if (_negocios == null)
                    throw new InvalidOperationException("NegocioRepository not initialized");
                return _negocios;
            }
        }

        // Persistencia
        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        // Alias para compatibilidad
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await CompleteAsync(cancellationToken);
        }

        // Gestión de transacciones
        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("Una transacción ya está en progreso.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No hay transacción activa para hacer commit.");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No hay transacción activa para hacer rollback.");
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        // Dispose pattern
        public void Dispose()
        {
            _currentTransaction?.Dispose();
            // No disposing context aquí, lo maneja el DI container
        }
    }
}

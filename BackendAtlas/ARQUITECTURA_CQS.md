# Arquitectura CQS (Command Query Separation) - BackendAtlas

## 📋 Resumen

Este proyecto implementa **CQS (Command Query Separation)** con **Unit of Work** y **repositorios específicos** (sin repositorio genérico).

## 🎯 Principios

### 1. **Separación de Responsabilidades**
- **QUERIES (Lecturas)**: Inyectar repositorio directamente → AsNoTracking() para performance
- **COMMANDS (Escrituras)**: Usar IUnitOfWork → Control transaccional + SaveChanges centralizado

### 2. **Repositorios Específicos (NO Genéricos)**
- ❌ **Anti-patrón evitado**: `IGenericRepository<T>` con CRUD genérico
- ✅ **Patrón correcto**: Repositorios con métodos semánticos de negocio

### 3. **Métodos Semánticos**
- Nombres en español que expresan **intención de negocio**
- Ejemplos:
  - ✅ `ObtenerCatalogoPorSucursalAsync(sucursalId)`
  - ✅ `ObtenerPedidosDelDiaAsync(fecha)`
  - ❌ `GetByIdAsync(id)` ← genérico, poco semántico

## 🏗️ Estructura

### Repositorios Específicos

#### IProductoRepository
```csharp
public interface IProductoRepository
{
    // QUERIES (sin tracking)
    Task<Producto?> ObtenerPorIdConCategoriaAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Producto>> ObtenerCatalogoPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Producto>> ObtenerTodosActivosAsync(CancellationToken cancellationToken = default);
    
    // COMMANDS (con tracking)
    Task<Producto> AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
    void Actualizar(Producto producto);
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default); // para updates
}
```

#### IPedidoRepository
```csharp
public interface IPedidoRepository
{
    // QUERIES (sin tracking)
    Task<Pedido?> ObtenerPorIdConDetallesAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pedido>> ObtenerPedidosDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pedido>> ObtenerPorSucursalYEstadoAsync(int sucursalId, string? estadoNombre, CancellationToken cancellationToken = default);
    Task<IEnumerable<Pedido>> ObtenerPorEstadoAsync(EstadoPedido estado, CancellationToken cancellationToken = default);
    
    // COMMANDS (con tracking)
    Task<Pedido> AgregarAsync(Pedido pedido, CancellationToken cancellationToken = default);
    Task ActualizarEstadoAsync(int pedidoId, int estadoId, CancellationToken cancellationToken = default);
    void Actualizar(Pedido pedido);
    Task<Pedido?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default); // para updates
}
```

#### ICategoriaRepository
```csharp
public interface ICategoriaRepository
{
    // QUERIES (sin tracking)
    Task<IEnumerable<Categoria>> ObtenerCategoriasActivasAsync(CancellationToken cancellationToken = default);
    
    // COMMANDS (con tracking)
    Task<Categoria> AgregarAsync(Categoria categoria, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);
}
```

### Unit of Work

#### IUnitOfWork
```csharp
public interface IUnitOfWork
{
    // Repositorios
    IProductoRepository Productos { get; }
    IPedidoRepository Pedidos { get; }
    ICategoriaRepository Categorias { get; }
    ISucursalRepository Sucursales { get; }
    IUsuarioRepository Usuarios { get; }
    
    // Persistencia
    Task<int> CompleteAsync(CancellationToken cancellationToken = default); // ← Método principal
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // Alias para compatibilidad
    
    // Transacciones
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

## 📝 Ejemplo de Servicio con CQS

### ProductoService

```csharp
public class ProductoService : IProductoService
{
    // CQS: Repo directo para LECTURAS
    private readonly IProductoRepository _productoRepository;
    
    // CQS: UnitOfWork para ESCRITURAS
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IMapper _mapper;

    public ProductoService(
        IProductoRepository productoRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _productoRepository = productoRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // ============ QUERIES (LECTURAS - AsNoTracking) ============

    public async Task<ProductoResponseDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // CQS: Repo directo para consulta rápida con AsNoTracking
        var producto = await _productoRepository.ObtenerPorIdConCategoriaAsync(id, cancellationToken);
        if (producto == null)
        {
            throw new KeyNotFoundException($"Producto con ID {id} no encontrado.");
        }
        return _mapper.Map<ProductoResponseDto>(producto);
    }

    public async Task<IEnumerable<ProductoResponseDto>> ObtenerPorSucursalAsync(int sucursalId, CancellationToken cancellationToken = default)
    {
        // CQS: Repo directo con método semántico
        var productos = await _productoRepository.ObtenerCatalogoPorSucursalAsync(sucursalId, cancellationToken);
        return _mapper.Map<IEnumerable<ProductoResponseDto>>(productos);
    }

    // ============ COMMANDS (ESCRITURAS - UnitOfWork) ============

    public async Task<ProductoResponseDto> CrearAsync(ProductoCreateRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Precio <= 0)
        {
            throw new ArgumentException("El precio debe ser mayor a 0.");
        }

        // Validar categoría (Repo directo para lectura)
        var categoriaExiste = await _unitOfWork.Categorias.ExisteAsync(request.CategoriaId, cancellationToken);
        if (!categoriaExiste)
        {
            throw new ArgumentException($"La categoría con ID {request.CategoriaId} no existe.");
        }

        var producto = _mapper.Map<Producto>(request);
        producto.Activo = true;

        // CQS: UnitOfWork para escritura transaccional
        await _unitOfWork.Productos.AgregarAsync(producto, cancellationToken);
        await _unitOfWork.CompleteAsync(cancellationToken); // ← Persistir cambios

        // Recargar con repo directo para devolver DTO
        var productoCreado = await _productoRepository.ObtenerPorIdConCategoriaAsync(producto.Id, cancellationToken);
        return _mapper.Map<ProductoResponseDto>(productoCreado);
    }

    public async Task<ProductoResponseDto> ActualizarAsync(ProductoUpdateRequestDto request, CancellationToken cancellationToken = default)
    {
        // Para actualizar necesitamos tracking, usamos UnitOfWork
        var productoExistente = await _unitOfWork.Productos.ObtenerPorIdAsync(request.Id, cancellationToken);
        if (productoExistente == null)
        {
            throw new KeyNotFoundException($"Producto con ID {request.Id} no encontrado.");
        }

        _mapper.Map(request, productoExistente);
        
        // CQS: UnitOfWork para escritura
        _unitOfWork.Productos.Actualizar(productoExistente);
        await _unitOfWork.CompleteAsync(cancellationToken);

        // Recargar con repo directo
        var productoActualizado = await _productoRepository.ObtenerPorIdConCategoriaAsync(request.Id, cancellationToken);
        return _mapper.Map<ProductoResponseDto>(productoActualizado);
    }
}
```

## 🔥 Ejemplo con Transacciones: PedidoService

```csharp
public async Task<PedidoResponseDto> GenerarPedidoAsync(PedidoCreateDto dto, CancellationToken cancellationToken = default)
{
    // Iniciar transacción para operación compleja
    await _unitOfWork.BeginTransactionAsync(cancellationToken);
    
    try
    {
        var pedido = new Pedido { /* ... */ };

        // Restar stock de productos
        foreach (var item in dto.Items)
        {
            var producto = await _unitOfWork.Productos.ObtenerPorIdAsync(item.ProductoId, cancellationToken);
            if (producto.Stock < item.Cantidad)
            {
                throw new ArgumentException($"Stock insuficiente para '{producto.Nombre}'.");
            }

            producto.Stock -= item.Cantidad;
            _unitOfWork.Productos.Actualizar(producto);
        }

        // Guardar pedido
        await _unitOfWork.Pedidos.AgregarAsync(pedido, cancellationToken);

        // Commit: Guarda pedido + productos actualizados ATÓMICAMENTE
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return _mapper.Map<PedidoResponseDto>(pedido);
    }
    catch
    {
        // Rollback: Si falla, NO se resta stock ni se crea pedido
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        throw;
    }
}
```

## 🎨 Ventajas de Esta Arquitectura

### ✅ Performance
- **AsNoTracking()** en queries → 30-40% más rápido
- Sin overhead de change tracking cuando solo leemos

### ✅ Transaccionalidad
- Control explícito de cuándo persistir cambios
- Transacciones complejas con rollback automático

### ✅ Semántica Clara
- Métodos con nombres de negocio: `ObtenerCatalogoPorSucursalAsync`
- No CRUD genérico: cada método tiene propósito claro

### ✅ Testabilidad
- Fácil mockear `IProductoRepository` para tests de lectura
- Fácil mockear `IUnitOfWork` para tests de escritura

### ✅ Separación de Responsabilidades
- Queries: Rápidas, sin transacciones, sin tracking
- Commands: Transaccionales, con tracking, con validaciones

## 📦 Registro DI (Dependency Injection)

```csharp
// Repositorios (Scoped para mantener contexto por request)
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<ISucursalRepository, SucursalRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// Unit of Work (Scoped para compartir AppDbContext con repositorios)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ISucursalService, SucursalService>();
builder.Services.AddScoped<IMaestrosService, MaestrosService>();
```

## 🚫 Anti-Patrones Evitados

### ❌ Repositorio Genérico
```csharp
// MAL: Métodos genéricos sin semántica
public interface IGenericRepository<T>
{
    Task<T> GetByIdAsync(int id); // ← ¿Con includes? ¿Sin tracking?
    Task<IEnumerable<T>> GetAllAsync(); // ← ¿Filtros? ¿Ordenamiento?
}
```

### ✅ Repositorio Específico
```csharp
// BIEN: Métodos semánticos con intención clara
public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdConCategoriaAsync(int id); // ← Incluye categoría, sin tracking
    Task<IEnumerable<Producto>> ObtenerCatalogoPorSucursalAsync(int sucursalId); // ← Solo activos, ordenados
}
```

### ❌ SaveChanges en Repositorio
```csharp
// MAL: Repository no debe controlar persistencia
public async Task<Producto> AddAsync(Producto producto)
{
    _context.Productos.Add(producto);
    await _context.SaveChangesAsync(); // ← Rompe transacciones
    return producto;
}
```

### ✅ SaveChanges en UnitOfWork/Service
```csharp
// BIEN: Servicio controla cuándo persistir
public async Task<ProductoResponseDto> CrearAsync(ProductoCreateRequestDto request)
{
    await _unitOfWork.Productos.AgregarAsync(producto);
    await _unitOfWork.CompleteAsync(); // ← Control centralizado
    return dto;
}
```

## 🔍 Resumen de Convenciones

| Tipo | Inyección | Tracking | SaveChanges | Ejemplo |
|------|-----------|----------|-------------|---------|
| **Query (Lectura)** | Repositorio directo | ❌ AsNoTracking | ❌ No | `ObtenerCatalogoPorSucursalAsync` |
| **Command (Escritura)** | UnitOfWork | ✅ Sí | ✅ CompleteAsync | `CrearAsync`, `ActualizarAsync` |
| **Transacción Compleja** | UnitOfWork | ✅ Sí | ✅ CommitTransactionAsync | `GenerarPedidoAsync` (stock + pedido) |

---

**Última actualización**: Enero 2025  
**Patrón**: CQS + Unit of Work + Repositorios Específicos  
**Anti-patrón evitado**: Generic Repository
